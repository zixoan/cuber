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

namespace Zixoan.Cuber.Server.Proxy.Udp
{
    public class UdpProxy : ProxyBase
    {
        private const int DefaultInactiveTicks = 15000 * 10000;

        private readonly ILogger logger;
        private readonly CuberOptions cuberOptions;
        private readonly EndPoint AnyEndPoint = new IPEndPoint(IPAddress.Any, 0);

        private Socket socket;
        private bool running;
        private string ip;
        private ushort port;
        private byte[] upStreamReceiveBuffer;

        private Timer inactiveTimer;
        private IDictionary<EndPoint, UdpConnectionState> clients = new ConcurrentDictionary<EndPoint, UdpConnectionState>();

        public UdpProxy(
            ILogger<UdpProxy> logger,
            IOptions<CuberOptions> options,
            ILoadBalanceStrategy loadBalanceStrategy) 
            : base(loadBalanceStrategy)
        {
            this.logger = logger;
            this.cuberOptions = options.Value;
        }

        public override void Listen(string ip, ushort port)
        {
            this.ip = ip;
            this.port = port;
            this.upStreamReceiveBuffer = new byte[this.cuberOptions.UpStreamBufferSize];
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            this.socket.Bind(new IPEndPoint(IPAddress.Parse(this.ip), this.port));
            this.running = true;

            this.inactiveTimer = new Timer(OnInactiveTimerTick, null, 5000, 5000);

            EndPoint endPoint = AnyEndPoint;
            this.socket.BeginReceiveFrom(this.upStreamReceiveBuffer, 0, this.upStreamReceiveBuffer.Length, SocketFlags.None, ref endPoint, new AsyncCallback(OnReceiveFromUpStream), null);

            this.logger.LogInformation($"Udp proxy listening on {this.ip}:{this.port}");
        }

        public override void Stop()
        {
            if (!this.running)
            {
                return;
            }

            foreach (var pair in this.clients)
            {
                pair.Value.Stop();
            }
            this.clients.Clear();

            this.socket.Close();

            this.inactiveTimer.Change(Timeout.Infinite, Timeout.Infinite);
            this.inactiveTimer.Dispose();

            this.running = false;

            this.logger.LogInformation("Udp proxy stopped");
        }

        private void OnReceiveFromUpStream(IAsyncResult ar)
        {
            try
            {
                EndPoint upStreamEndpoint = AnyEndPoint;
                int received = this.socket.EndReceiveFrom(ar, ref upStreamEndpoint);

                if (this.clients.TryGetValue(upStreamEndpoint, out UdpConnectionState state))
                {
                    state.Socket.BeginSendTo(this.upStreamReceiveBuffer, 0, received, SocketFlags.None, state.DownStreamEndPoint, new AsyncCallback(OnSendToDownStream), state);
                    EndPoint targetEndPoint = state.DownStreamEndPoint;
                    state.Socket.BeginReceiveFrom(state.DownStreamReceiveBuffer, 0, state.DownStreamReceiveBuffer.Length, SocketFlags.None, ref targetEndPoint, new AsyncCallback(OnReceiveFromDownStream), state);

                    state.LastActivity = DateTime.Now.Ticks;
                }
                else
                {
                    Target target = this.loadBalanceStrategy.GetTarget();
                    if (target == null)
                    {
                        this.logger.LogError($"Closed connection: Client [{upStreamEndpoint}] because no target was available");
                        return;
                    }

                    EndPoint downStreamEndPoint = new IPEndPoint(IPAddress.Parse(target.Ip), target.Port);
                    UdpConnectionState udpConnectionState = new UdpConnectionState
                    {
                        Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp),
                        Target = target,
                        UpStreamEndPoint = upStreamEndpoint,
                        DownStreamEndPoint = downStreamEndPoint,
                        DownStreamReceiveBuffer = new byte[this.cuberOptions.DownStreamBufferSize]
                    };
                    udpConnectionState.Socket.Bind(new IPEndPoint(((IPEndPoint)this.socket.LocalEndPoint).Address, 0));

                    this.clients.Add(upStreamEndpoint, udpConnectionState);

                    target.IncrementConnections();

                    udpConnectionState.Socket.BeginSendTo(this.upStreamReceiveBuffer, 0, received, SocketFlags.None, udpConnectionState.DownStreamEndPoint, new AsyncCallback(OnSendToDownStream), udpConnectionState);
                    udpConnectionState.Socket.BeginReceiveFrom(udpConnectionState.DownStreamReceiveBuffer, 0, udpConnectionState.DownStreamReceiveBuffer.Length, SocketFlags.None, ref downStreamEndPoint, new AsyncCallback(OnReceiveFromDownStream), udpConnectionState);

                    this.logger.LogDebug($"New connection: Client [{upStreamEndpoint}] <-> Proxy [{this.ip}:{this.port}] <-> Target [{downStreamEndPoint}]");
                }

                EndPoint endPoint = AnyEndPoint;
                this.socket.BeginReceiveFrom(this.upStreamReceiveBuffer, 0, this.upStreamReceiveBuffer.Length, SocketFlags.None, ref endPoint, new AsyncCallback(OnReceiveFromUpStream), null);
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Error in receive from up stream, stopping udp proxy");

                this.Stop();
            }
        }

        private void OnSendToDownStream(IAsyncResult ar)
        {
            UdpConnectionState state = (UdpConnectionState)ar.AsyncState;

            try
            {
                _ = state.Socket.EndSendTo(ar);
            }
            catch (Exception)
            {
                this.Stop(state);
            }
        }

        private void OnReceiveFromDownStream(IAsyncResult ar)
        {
            UdpConnectionState state = (UdpConnectionState)ar.AsyncState;

            try
            {
                EndPoint endPoint = state.DownStreamEndPoint;
                int received = state.Socket.EndReceiveFrom(ar, ref endPoint);

                this.socket.BeginSendTo(state.DownStreamReceiveBuffer, 0, received, SocketFlags.None, state.UpStreamEndPoint, new AsyncCallback(OnSendToUpStream), state);

                state.LastActivity = DateTime.Now.Ticks;
            }
            catch (Exception)
            {
                this.Stop(state);
            }
        }

        private void OnSendToUpStream(IAsyncResult ar)
        {
            UdpConnectionState state = (UdpConnectionState)ar.AsyncState;

            try
            {
                _ = this.socket.EndSendTo(ar);
            }
            catch (Exception)
            {
                this.Stop(state);
            }
        }

        private void OnInactiveTimerTick(object state)
        {
            foreach (KeyValuePair<EndPoint, UdpConnectionState> pair in this.clients)
            {
                // TODO: Configurable inactive/timeout milliseconds for idle UDP "connections"
                if (DateTime.Now.Ticks - pair.Value.LastActivity >= DefaultInactiveTicks)
                {
                    this.Stop(pair.Value);

                    this.clients.Remove(pair.Key);
                }
            }
        }

        private void Stop(UdpConnectionState state)
        {
            bool wasConnected = state.Connected;

            if (state.Stop() && wasConnected)
            {
                state.Target.DecrementConnections();

                this.logger.LogDebug($"Closed connection: Client [{state.UpStreamEndPoint}] <-> Proxy [{this.ip}:{this.port}] <-> Target [{state.DownStreamEndPoint}]");
            }
        }
    }
}
