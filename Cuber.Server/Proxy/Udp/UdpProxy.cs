using System;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zixoan.Cuber.Server.Balancing;
using Zixoan.Cuber.Server.Config;

namespace Zixoan.Cuber.Server.Proxy.Udp
{
    public class UdpProxy : ProxyBase
    {
        private readonly ILogger logger;
        private readonly CuberOptions cuberOptions;
        private readonly EndPoint AnyEndPoint = new IPEndPoint(IPAddress.Any, 0);

        private Socket socket;
        private bool running;

        private string ip;
        private ushort port;
        private byte[] upStreamReceiveBuffer;

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

            this.logger.LogInformation($"Udp proxy listening on {this.ip}:{this.port}");

            EndPoint endPoint = AnyEndPoint;
            this.socket.BeginReceiveFrom(this.upStreamReceiveBuffer, 0, this.upStreamReceiveBuffer.Length, SocketFlags.None, ref endPoint, new AsyncCallback(OnReceiveFromUpStream), null);
        }

        public override void Stop()
        {
            if (!this.running)
            {
                return;
            }

            this.running = false;
            this.socket.Close();

            this.logger.LogInformation("Udp proxy stopped");
        }

        private void OnReceiveFromUpStream(IAsyncResult ar)
        {
            try
            {
                EndPoint clientEndpoint = AnyEndPoint;
                int received = this.socket.EndReceiveFrom(ar, ref clientEndpoint);

                Target target = this.loadBalanceStrategy.GetTarget();
                if (target == null)
                {
                    this.logger.LogError($"Closed connection: Client [{clientEndpoint}] because no target was available");
                    return;
                }

                UdpMessageContext context = new UdpMessageContext
                {
                    ClientEndpoint = clientEndpoint,
                    TargetEndPoint = new IPEndPoint(IPAddress.Parse(target.Ip), target.Port),
                    DownStreamReceiveBuffer = new byte[this.cuberOptions.DownStreamBufferSize]
                };
                this.socket.BeginSendTo(this.upStreamReceiveBuffer, 0, received, SocketFlags.None, context.TargetEndPoint, new AsyncCallback(OnSendToDownStream), context);

                EndPoint endPoint = AnyEndPoint;
                this.socket.BeginReceiveFrom(this.upStreamReceiveBuffer, 0, this.upStreamReceiveBuffer.Length, SocketFlags.None, ref endPoint, new AsyncCallback(OnReceiveFromUpStream), null);
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Error in receive from up stream, stopping udp proxy");

                Stop();
            }
        }

        private void OnSendToDownStream(IAsyncResult ar)
        {
            UdpMessageContext context = (UdpMessageContext)ar.AsyncState;

            int sent = this.socket.EndSendTo(ar);

            EndPoint endPoint = context.TargetEndPoint;
            this.socket.BeginReceiveFrom(context.DownStreamReceiveBuffer, 0, context.DownStreamReceiveBuffer.Length, SocketFlags.None, ref endPoint, new AsyncCallback(OnReceiveFromDownStream), context);
        }

        private void OnReceiveFromDownStream(IAsyncResult ar)
        {
            UdpMessageContext context = (UdpMessageContext)ar.AsyncState;

            EndPoint endPoint = context.TargetEndPoint;
            int received = this.socket.EndReceiveFrom(ar, ref endPoint);

            this.socket.BeginSendTo(context.DownStreamReceiveBuffer, 0, received, SocketFlags.None, context.ClientEndpoint, new AsyncCallback(OnSendToUpStream), null);
        }

        private void OnSendToUpStream(IAsyncResult ar)
        {
            int sent = this.socket.EndSendTo(ar);
        }
    }
}
