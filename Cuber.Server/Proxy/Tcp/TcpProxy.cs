using System;
using System.Net;
using System.Net.Sockets;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Zixoan.Cuber.Server.Balancing;
using Zixoan.Cuber.Server.Config;
using Zixoan.Cuber.Server.Stats;

namespace Zixoan.Cuber.Server.Proxy.Tcp
{
    public class TcpProxy : ProxyBase
    {
        private readonly ILogger logger;
        private readonly CuberOptions cuberOptions;

        private Socket socket;
        private bool running;

        private string ip;
        private ushort port;

        private IStatsService statsService;
        private ProxyStats proxyStats;

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
        }

        public override void Start(string ip, ushort port)
        {
            this.ip = ip;
            this.port = port;
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.socket.Bind(new IPEndPoint(IPAddress.Parse(this.ip), this.port));
            this.socket.Listen(this.cuberOptions.Backlog);
            this.running = true;

            this.proxyStats = new ProxyStats(DateTimeOffset.Now.ToUnixTimeSeconds());
            this.statsService.Add("tcp", this.proxyStats);

            this.logger.LogInformation($"Tcp proxy listening on {this.ip}:{this.port}");

            this.socket.BeginAccept(new AsyncCallback(OnAccept), null);
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
                Socket socket = this.socket.EndAccept(ar);

                Target target = this.loadBalanceStrategy.GetTarget(socket.RemoteEndPoint);
                if (target == null)
                {
                    this.logger.LogError($"Closed connection: Client [{socket.RemoteEndPoint}] because no target was available");
                    socket.Close();
                    return;
                }

                TcpConnectionState state = new TcpConnectionState()
                {
                    UpStreamSocket = socket,
                    DownStreamSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp),
                    UpStreamBuffer = new byte[this.cuberOptions.UpStreamBufferSize],
                    DownStreamBuffer = new byte[this.cuberOptions.DownStreamBufferSize],
                    Target = target
                };
                state.DownStreamSocket.BeginConnect(target.Ip, target.Port, new AsyncCallback(OnDownstreamConnect), state);

                this.socket.BeginAccept(new AsyncCallback(OnAccept), null);
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "Error in accept callback, stopping tcp proxy");

                this.Stop();
            }
        }

        private void OnDownstreamConnect(IAsyncResult ar)
        {
            TcpConnectionState state = (TcpConnectionState)ar.AsyncState;

            try
            {
                state.DownStreamSocket.EndConnect(ar);

                if (state.DownStreamSocket.Connected)
                {
                    state.Connected = true;
                    state.Target.IncrementConnections();

                    state.UpStreamEndPoint = state.UpStreamSocket.RemoteEndPoint.ToString();
                    state.DownStreamEndPoint = state.DownStreamSocket.RemoteEndPoint.ToString();

                    this.proxyStats.IncrementActiveConnections();

                    this.logger.LogDebug($"New connection: Client [{state.UpStreamEndPoint}] <-> Cuber Proxy [{this.ip}:{this.port}] <-> Target [{state.DownStreamEndPoint}]");

                    state.UpStreamSocket.BeginReceive(state.UpStreamBuffer, 0, state.UpStreamBuffer.Length, SocketFlags.None, new AsyncCallback(OnReceiveUpstream), state);
                    state.DownStreamSocket.BeginReceive(state.DownStreamBuffer, 0, state.DownStreamBuffer.Length, SocketFlags.None, new AsyncCallback(OnReceiveDownstream), state);
                }
                else
                {
                    this.Close(state);
                }
            }
            catch (Exception)
            {
                this.Close(state);
            }
        }

        private void OnReceiveDownstream(IAsyncResult ar)
        {
            TcpConnectionState state = (TcpConnectionState)ar.AsyncState;

            try
            {
                int received = state.DownStreamSocket.EndReceive(ar);
                if (received == 0)
                {
                    this.Close(state);
                    return;
                }

                this.proxyStats.IncrementDownstreamReceived(received);

                state.UpStreamSocket.BeginSend(state.DownStreamBuffer, 0, received, SocketFlags.None, new AsyncCallback(OnSendUpstream), state);
            }
            catch (Exception)
            {
                this.Close(state);
            }
        }

        private void OnSendUpstream(IAsyncResult ar)
        {
            TcpConnectionState state = (TcpConnectionState)ar.AsyncState;

            try
            {
                int sent = state.UpStreamSocket.EndSend(ar);

                this.proxyStats.IncrementUpstreamSent(sent);

                state.DownStreamSocket.BeginReceive(state.DownStreamBuffer, 0, state.DownStreamBuffer.Length, SocketFlags.None, new AsyncCallback(OnReceiveDownstream), state);
            }
            catch (Exception)
            {
                this.Close(state);
            }
        }

        private void OnReceiveUpstream(IAsyncResult ar)
        {
            TcpConnectionState state = (TcpConnectionState)ar.AsyncState;

            try
            {
                int received = state.UpStreamSocket.EndReceive(ar);
                if (received == 0)
                {
                    this.Close(state);
                    return;
                }

                this.proxyStats.IncrementUpstreamReceived(received);

                state.DownStreamSocket.BeginSend(state.UpStreamBuffer, 0, received, SocketFlags.None, new AsyncCallback(OnSendDownstream), state);
            }
            catch (Exception)
            {
                this.Close(state);
            }
        }

        private void OnSendDownstream(IAsyncResult ar)
        {
            TcpConnectionState state = (TcpConnectionState)ar.AsyncState;

            try
            {
                int sent = state.DownStreamSocket.EndSend(ar);

                this.proxyStats.IncrementDownstreamSent(sent);

                state.UpStreamSocket.BeginReceive(state.UpStreamBuffer, 0, state.UpStreamBuffer.Length, SocketFlags.None, new AsyncCallback(OnReceiveUpstream), state);
            }
            catch (Exception)
            {
                this.Close(state);
            }
        }

        private void Close(TcpConnectionState state)
        {
            bool wasConnected = state.Connected;

            if (state.Close() && wasConnected)
            {
                state.Target.DecrementConnections();

                this.proxyStats.DecrementActiveConnections();

                this.logger.LogDebug($"Closed connection: Client [{state.UpStreamEndPoint}] <-> Cuber Proxy [{this.ip}:{this.port}] <-> Target [{state.DownStreamEndPoint}]");
            }
        }
    }
}
