using System.Net.Sockets;

namespace Zixoan.Cuber.Server.Proxy.Tcp
{
    public class ProxyConnectionState
    {
        public readonly object stateLock = new object();

        private bool connected;
        private bool closed;

        public Socket UpStreamSocket { get; set; }
        public Socket DownStreamSocket { get; set; }

        public byte[] UpStreamBuffer { get; set; }
        public byte[] DownStreamBuffer { get; set; }

        public string UpStreamEndPoint { get; set; }
        public string DownStreamEndPoint { get; set; }
        public bool Connected
        {
            get
            {
                lock (this.stateLock)
                {
                    return this.connected && !this.closed;
                }
            }
            set
            {
                lock (this.stateLock)
                {
                    this.connected = value;
                }
            }
        }

        public bool Close()
        {
            lock (this.stateLock)
            {
                if (this.closed)
                {
                    return false;
                }

                this.closed = true;
                this.connected = false;

                UpStreamSocket.Close();
                DownStreamSocket.Close();

                return true;
            }
        }
    }
}
