using System.Net;

namespace Zixoan.Cuber.Server.Proxy.Udp
{
    public class UdpMessageContext
    {
        public EndPoint ClientEndpoint { get; set; }
        public EndPoint TargetEndPoint { get; set; }
        public byte[] DownStreamReceiveBuffer { get; set; }
    }
}
