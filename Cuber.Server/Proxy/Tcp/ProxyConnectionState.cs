using System.Net.Sockets;

namespace Zixoan.Cuber.Server.Proxy.Tcp
{
    public class ProxyConnectionState
    {
        private readonly object closedLock = new object();

        private bool closed;

        public Socket UpStreamSocket { get; set; }
        public Socket DownStreamSocket { get; set; }

        public byte[] UpStreamBuffer { get; set; } = new byte[4096];
        public byte[] DownStreamBuffer { get; set; } = new byte[4096];

        public string UpStreamEndPoint { get; set; }
        public string DownStreamEndPoint { get; set; }

        public bool Close()
        {
            lock (closedLock)
            {
                if (this.closed)
                {
                    return false;
                }

                this.closed = true;

                UpStreamSocket.Close();
                DownStreamSocket.Close();

                return true;
            }
        }
    }
}
