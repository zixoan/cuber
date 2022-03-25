using System;
using System.Net;
using System.Net.Sockets;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Zixoan.Cuber.Server.Balancing;
using Zixoan.Cuber.Server.Config;
using Zixoan.Cuber.Server.Stats;

namespace Zixoan.Cuber.Server.Proxy.Tcp;

public class TcpProxy : ProxyBase
{
    private readonly ILogger logger;
    private readonly CuberOptions cuberOptions;
    private readonly IStatsService statsService;
    private readonly Socket socket;

    private bool running;

    private readonly string ip;
    private readonly ushort port;

    private readonly ProxyStats proxyStats;

    public TcpProxy(
        ILogger<TcpProxy> logger,
        IOptions<CuberOptions> options,
        IStatsService statsService,
        ILoadBalanceStrategy loadBalanceStrategy) 
        : base(loadBalanceStrategy)
    {
        this.logger = logger;
        this.cuberOptions = options.Value;
        this.statsService = statsService;
        this.proxyStats = new ProxyStats(DateTimeOffset.Now.ToUnixTimeSeconds());
        this.statsService.Add("tcp", this.proxyStats);
        this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        this.ip = this.cuberOptions.Ip;
        this.port = this.cuberOptions.Port;
    }

    public override void Start()
    {
        this.socket.Bind(new IPEndPoint(IPAddress.Parse(this.ip), this.port));
        this.socket.Listen(this.cuberOptions.Backlog);
        this.running = true;

        this.logger.LogInformation("Tcp proxy listening on {ip}:{port}", this.ip, this.port);

        this.socket.BeginAccept(this.OnAccept, null);
    }

    public override void Stop()
    {
        if (!this.running)
        {
            return;
        }

        this.running = false;
        this.socket.Close();

        this.statsService.Remove("tcp");

        this.logger.LogInformation("Tcp proxy stopped");
    }

    private void OnAccept(IAsyncResult ar)
    {
        try
        {
            Socket acceptedSocket = this.socket.EndAccept(ar);

            Target? target = this.loadBalanceStrategy.GetTarget(acceptedSocket.RemoteEndPoint);
            if (target == null)
            {
                this.logger.LogError(
                    "Closed connection: Client [{remoteEndPoint}] because no target was available", acceptedSocket.RemoteEndPoint);
                acceptedSocket.Close();
                return;
            }

            EndPoint downStreamEndPoint = new IPEndPoint(IPAddress.Parse(target.Ip), target.Port);
            var tcpConnectionState = new TcpConnectionState(
                acceptedSocket,
                new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp),
                new byte[this.cuberOptions.UpStreamBufferSize],
                new byte[this.cuberOptions.DownStreamBufferSize],
                acceptedSocket.RemoteEndPoint!,
                downStreamEndPoint,
                target
            );
            tcpConnectionState.DownStreamSocket.BeginConnect(downStreamEndPoint, this.OnDownstreamConnect, tcpConnectionState);

            this.socket.BeginAccept(this.OnAccept, null);
        }
        catch (Exception exception)
        {
            this.logger.LogError(exception, "Error in accept callback, stopping tcp proxy");

            this.Stop();
        }
    }

    private void OnDownstreamConnect(IAsyncResult ar)
    {
        TcpConnectionState connectionState = (TcpConnectionState)ar.AsyncState!;

        try
        {
            connectionState.DownStreamSocket.EndConnect(ar);

            if (connectionState.DownStreamSocket.Connected)
            {
                connectionState.Connected = true;
                connectionState.Target.IncrementConnections();

                this.proxyStats.IncrementActiveConnections();

                this.logger.LogDebug(
                    "New connection: Client [{state.UpStreamEndPoint}] <-> Cuber Proxy [{this.ip}:{this.port}] <-> Target [{state.DownStreamEndPoint}]",
                    connectionState.UpStreamEndPoint,
                    this.ip,
                    this.port,
                    connectionState.DownStreamEndPoint);

                connectionState.UpStreamSocket.BeginReceive(connectionState.UpStreamBuffer, 0,
                    connectionState.UpStreamBuffer.Length, SocketFlags.None, this.OnReceiveUpstream, connectionState);
                connectionState.DownStreamSocket.BeginReceive(connectionState.DownStreamBuffer, 0,
                    connectionState.DownStreamBuffer.Length, SocketFlags.None, this.OnReceiveDownstream, connectionState);
            }
            else
            {
                this.Close(connectionState);
            }
        }
        catch (Exception)
        {
            this.Close(connectionState);
        }
    }

    private void OnReceiveDownstream(IAsyncResult ar)
    {
        TcpConnectionState connectionState = (TcpConnectionState)ar.AsyncState!;

        try
        {
            int received = connectionState.DownStreamSocket.EndReceive(ar);
            if (received == 0)
            {
                this.Close(connectionState);
                return;
            }

            this.proxyStats.IncrementDownstreamReceived(received);

            connectionState.UpStreamSocket.BeginSend(connectionState.DownStreamBuffer, 0, received,
                SocketFlags.None, this.OnSendUpstream, connectionState);
        }
        catch (Exception)
        {
            this.Close(connectionState);
        }
    }

    private void OnSendUpstream(IAsyncResult ar)
    {
        TcpConnectionState connectionState = (TcpConnectionState)ar.AsyncState!;

        try
        {
            int sent = connectionState.UpStreamSocket.EndSend(ar);

            this.proxyStats.IncrementUpstreamSent(sent);

            connectionState.DownStreamSocket.BeginReceive(connectionState.DownStreamBuffer, 0,
                connectionState.DownStreamBuffer.Length, SocketFlags.None, this.OnReceiveDownstream, connectionState);
        }
        catch (Exception)
        {
            this.Close(connectionState);
        }
    }

    private void OnReceiveUpstream(IAsyncResult ar)
    {
        TcpConnectionState connectionState = (TcpConnectionState)ar.AsyncState!;

        try
        {
            int received = connectionState.UpStreamSocket.EndReceive(ar);
            if (received == 0)
            {
                this.Close(connectionState);
                return;
            }

            this.proxyStats.IncrementUpstreamReceived(received);

            connectionState.DownStreamSocket.BeginSend(connectionState.UpStreamBuffer, 0, received,
                SocketFlags.None, this.OnSendDownstream, connectionState);
        }
        catch (Exception)
        {
            this.Close(connectionState);
        }
    }

    private void OnSendDownstream(IAsyncResult ar)
    {
        TcpConnectionState connectionState = (TcpConnectionState)ar.AsyncState!;

        try
        {
            int sent = connectionState.DownStreamSocket.EndSend(ar);

            this.proxyStats.IncrementDownstreamSent(sent);

            connectionState.UpStreamSocket.BeginReceive(connectionState.UpStreamBuffer, 0,
                connectionState.UpStreamBuffer.Length, SocketFlags.None, this.OnReceiveUpstream, connectionState);
        }
        catch (Exception)
        {
            this.Close(connectionState);
        }
    }

    private void Close(ConnectionStateBase state)
    {
        bool wasConnected = state.Connected;
        if (!state.Close() || !wasConnected)
        {
            return;
        }

        state.Target.DecrementConnections();
        this.proxyStats.DecrementActiveConnections();
        this.logger.LogDebug(
            "Closed connection: Client [{upStreamEndPoint}] <-> Cuber Proxy [{ip}:{port}] <-> Target [{downStreamEndPoint}]",
            state.UpStreamEndPoint, this.ip, this.port, state.DownStreamEndPoint);
    }
}