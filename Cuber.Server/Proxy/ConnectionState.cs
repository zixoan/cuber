using System.Net;
using System.Net.Sockets;

using Zixoan.Cuber.Server.Config;

namespace Zixoan.Cuber.Server.Proxy
{
    public abstract class ConnectionState
    {
        protected readonly object stateLock = new object();

        protected bool connected;

        public Socket UpStreamSocket { get; set; }
        public Socket DownStreamSocket { get; set; }

        public byte[] UpStreamBuffer { get; set; }
        public byte[] DownStreamBuffer { get; set; }

        public EndPoint UpStreamEndPoint { get; set; }
        public EndPoint DownStreamEndPoint { get; set; }

        public Target Target { get; set; }

        public bool Connected
        {
            get
            {
                lock (this.stateLock)
                {
                    return this.connected;
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

        public long LastActivity { get; set; }

        public ConnectionState(
            Socket upStreamSocket,
            Socket downStreamSocket,
            byte[] upStreamBuffer,
            byte[] downStreamBuffer,
            EndPoint upStreamEndPoint,
            EndPoint downStreamEndPoint,
            Target target)
        {
            this.UpStreamSocket = upStreamSocket;
            this.DownStreamSocket = downStreamSocket;
            this.UpStreamBuffer = upStreamBuffer;
            this.DownStreamBuffer = downStreamBuffer;
            this.UpStreamEndPoint = upStreamEndPoint;
            this.DownStreamEndPoint = downStreamEndPoint;
            this.Target = target;
        }

        public abstract bool Close();
    }
}
