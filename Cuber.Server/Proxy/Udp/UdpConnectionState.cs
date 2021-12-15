using System.Net;
using System.Net.Sockets;

using Zixoan.Cuber.Server.Config;

namespace Zixoan.Cuber.Server.Proxy.Udp
{
    public class UdpConnectionState
    {
        private readonly object stateLock = new object();

        public bool Connected
        {
            get
            {
                lock (this.stateLock)
                {
                    return !this.closed;
                }
            }
        }
        public Socket DownStreamSocket { get; set; }
        public Target? Target { get; set; }
        public EndPoint UpStreamEndPoint { get; set; }
        public byte[] DownStreamReceiveBuffer { get; set; }
        public EndPoint DownStreamEndPoint { get; set; }

        public long LastActivity { get; set; }

        private bool closed;

        public bool Stop()
        {
            lock (this.stateLock)
            {
                if (this.closed)
                {
                    return false;
                }

                this.DownStreamSocket.Close();
                this.closed = true;

                return true;
            }
        }
    }
}
