using System.Net;
using System.Net.Sockets;

using Zixoan.Cuber.Server.Config;

namespace Zixoan.Cuber.Server.Proxy.Udp
{
    public class UdpConnectionState : ConnectionStateBase
    {
        public UdpConnectionState(
            Socket upStreamSocket,
            Socket downStreamSocket,
            byte[] upStreamBuffer,
            byte[] downStreamBuffer,
            EndPoint upStreamEndPoint,
            EndPoint downStreamEndPoint,
            Target target)
            : base(
                upStreamSocket,
                downStreamSocket,
                upStreamBuffer,
                downStreamBuffer,
                upStreamEndPoint,
                downStreamEndPoint,
                target)
        {
        }

        public override bool Close()
        {
            lock (this.stateLock)
            {
                if (!this.connected)
                {
                    return false;
                }

                this.connected = false;

                this.DownStreamSocket.Close();

                return true;
            }
        }
    }
}
