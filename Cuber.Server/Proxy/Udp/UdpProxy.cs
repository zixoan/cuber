using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Zixoan.Cuber.Server.Balancing;
using Zixoan.Cuber.Server.Config;
using Zixoan.Cuber.Server.Stats;

namespace Zixoan.Cuber.Server.Proxy.Udp
{
    public class UdpProxy : ProxyBase
    {
        private const int DefaultInactiveTicks = 15000 * 10000;

        private readonly ILogger logger;
        private readonly CuberOptions cuberOptions;
        private readonly EndPoint anyEndPoint = new IPEndPoint(IPAddress.Any, 0);

        private readonly Socket socket;
        private bool running;
        private readonly string ip;
        private readonly ushort port;
        private readonly byte[] upStreamReceiveBuffer;

        private Timer? inactiveTimer;
        private readonly IDictionary<EndPoint, UdpConnectionState> clients = new ConcurrentDictionary<EndPoint, UdpConnectionState>();

        private readonly IStatsService statsService;
        private readonly ProxyStats udpStats;

        public UdpProxy(
            ILogger<UdpProxy> logger,
            IOptions<CuberOptions> options,
            IStatsService statsService,
            ILoadBalanceStrategy loadBalanceStrategy) 
            : base(loadBalanceStrategy)
        {
            this.logger = logger;
            this.cuberOptions = options.Value;
            this.statsService = statsService;
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            this.udpStats = new ProxyStats(DateTimeOffset.Now.ToUnixTimeSeconds());
            this.statsService.Add("udp", this.udpStats);
            this.upStreamReceiveBuffer = new byte[this.cuberOptions.UpStreamBufferSize];
            this.ip = this.cuberOptions.Ip;
            this.port = this.cuberOptions.Port;
        }

        public override void Start()
        {
            this.socket.Bind(new IPEndPoint(IPAddress.Parse(this.ip), this.port));
            this.running = true;

            this.inactiveTimer = new Timer(OnInactiveTimerTick, null, 5000, 5000);

            EndPoint endPoint = this.anyEndPoint;
            this.socket.BeginReceiveFrom(this.upStreamReceiveBuffer, 0, this.upStreamReceiveBuffer.Length, SocketFlags.None, ref endPoint, this.OnReceiveFromUpstream, null);

            this.logger.LogInformation("Udp proxy listening on {ip}:{port}", this.ip, this.port);
        }

        public override void Stop()
        {
            if (!this.running)
            {
                return;
            }

            foreach (var (_, udpConnectionState) in this.clients)
            {
                udpConnectionState.Close();
            }
            this.clients.Clear();

            this.socket.Close();

            if (this.inactiveTimer != null)
            {
                this.inactiveTimer.Change(Timeout.Infinite, Timeout.Infinite);
                this.inactiveTimer.Dispose();
            }

            this.running = false;

            this.statsService.Remove("udp");

            this.logger.LogInformation("Udp proxy stopped");
        }

        private void OnReceiveFromUpstream(IAsyncResult ar)
        {
            try
            {
                EndPoint upStreamEndpoint = this.anyEndPoint;
                int received = this.socket.EndReceiveFrom(ar, ref upStreamEndpoint);

                this.udpStats.IncrementUpstreamReceived(received);

                if (this.clients.TryGetValue(upStreamEndpoint, out UdpConnectionState? state))
                {
                    state.DownStreamSocket.BeginSendTo(this.upStreamReceiveBuffer, 0, received, SocketFlags.None, state.DownStreamEndPoint, this.OnSendToDownstream, state);
                    EndPoint targetEndPoint = state.DownStreamEndPoint;
                    state.DownStreamSocket.BeginReceiveFrom(state.DownStreamBuffer, 0, state.DownStreamBuffer.Length, SocketFlags.None, ref targetEndPoint, this.OnReceiveFromDownstream, state);

                    state.LastActivity = DateTime.Now.Ticks;
                }
                else
                {
                    Target? target = this.loadBalanceStrategy.GetTarget(upStreamEndpoint);
                    if (target == null)
                    {
                        this.logger.LogError("Closed connection: Client [{upStreamEndpoint}] because no target was available", upStreamEndpoint);
                        return;
                    }

                    EndPoint downStreamEndPoint = new IPEndPoint(IPAddress.Parse(target.Ip), target.Port);
                    var udpConnectionState = new UdpConnectionState(
                        this.socket,
                        new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp),
                        new byte[this.cuberOptions.UpStreamBufferSize],
                        new byte[this.cuberOptions.DownStreamBufferSize],
                        upStreamEndpoint,
                        downStreamEndPoint,
                        target
                    );
                    udpConnectionState.DownStreamSocket.Connect(downStreamEndPoint);

                    udpConnectionState.Connected = true;

                    this.clients.Add(upStreamEndpoint, udpConnectionState);

                    udpConnectionState.DownStreamSocket.BeginSendTo(this.upStreamReceiveBuffer, 0, received, SocketFlags.None, udpConnectionState.DownStreamEndPoint, this.OnSendToDownstream, udpConnectionState);
                    udpConnectionState.DownStreamSocket.BeginReceiveFrom(udpConnectionState.DownStreamBuffer, 0, udpConnectionState.DownStreamBuffer.Length, SocketFlags.None, ref downStreamEndPoint, this.OnReceiveFromDownstream, udpConnectionState);
                    
                    target.IncrementConnections();
                    
                    this.udpStats.IncrementActiveConnections();
                    
                    this.logger.LogDebug(
                        "New connection: Client [{upStreamEndpoint}] <-> Cuber Proxy [{ip}:{port}] <-> Target [{downStreamEndPoint}]",
                        upStreamEndpoint, this.ip, this.port, downStreamEndPoint);
                }

                EndPoint endPoint = this.anyEndPoint;
                this.socket.BeginReceiveFrom(this.upStreamReceiveBuffer, 0, this.upStreamReceiveBuffer.Length, SocketFlags.None, ref endPoint, this.OnReceiveFromUpstream, null);
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Error in receive from up stream, stopping udp proxy");

                this.Stop();
            }
        }

        private void OnSendToDownstream(IAsyncResult ar)
        {
            UdpConnectionState state = (UdpConnectionState)ar.AsyncState!;

            try
            {
                int sent = state.DownStreamSocket.EndSendTo(ar);

                this.udpStats.IncrementDownstreamSent(sent);
            }
            catch (Exception)
            {
                this.Close(state);
            }
        }

        private void OnReceiveFromDownstream(IAsyncResult ar)
        {
            UdpConnectionState state = (UdpConnectionState)ar.AsyncState!;

            try
            {
                EndPoint endPoint = state.DownStreamEndPoint;
                int received = state.DownStreamSocket.EndReceiveFrom(ar, ref endPoint);

                this.udpStats.IncrementDownstreamReceived(received);

                this.socket.BeginSendTo(state.DownStreamBuffer, 0, received, SocketFlags.None, state.UpStreamEndPoint, this.OnSendToUpstream, state);

                state.LastActivity = DateTime.Now.Ticks;
            }
            catch (Exception)
            {
                this.Close(state);
            }
        }

        private void OnSendToUpstream(IAsyncResult ar)
        {
            UdpConnectionState state = (UdpConnectionState)ar.AsyncState!;

            try
            {
                int sent = this.socket.EndSendTo(ar);

                this.udpStats.IncrementUpstreamSent(sent);
            }
            catch (Exception)
            {
                this.Close(state);
            }
        }

        private void OnInactiveTimerTick(object? state)
        {
            foreach (var (endPoint, udpConnectionState) in this.clients)
            {
                // TODO: Configurable inactive/timeout milliseconds for idle UDP "connections"
                if (DateTime.Now.Ticks - udpConnectionState.LastActivity >= DefaultInactiveTicks)
                {
                    this.Close(udpConnectionState);
            
                    this.clients.Remove(endPoint);
                }
            }
        }

        private void Close(ConnectionStateBase state)
        {
            bool wasConnected = state.Connected;

            if (state.Close() && wasConnected)
            {
                state.Target.DecrementConnections();

                this.udpStats.DecrementActiveConnections();

                this.logger.LogDebug(
                    "Closed connection: Client [{upStreamEndPoint}] <-> Cuber Proxy [{ip}:{port}] <-> Target [{downStreamEndPoint}]",
                    state.UpStreamEndPoint, this.ip, this.port, state.DownStreamEndPoint);
            }
        }
    }
}
