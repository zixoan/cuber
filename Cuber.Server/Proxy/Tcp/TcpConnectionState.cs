using System.Net;
using System.Net.Sockets;

using Zixoan.Cuber.Server.Config;

namespace Zixoan.Cuber.Server.Proxy.Tcp
{
    public class TcpConnectionState : ConnectionState
    {
        public TcpConnectionState(
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

                this.UpStreamSocket.Close();
                this.DownStreamSocket.Close();

                return true;
            }
        }
    }
}
