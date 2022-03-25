using System.Net;
using System.Net.Sockets;

using Zixoan.Cuber.Server.Config;

namespace Zixoan.Cuber.Server.Proxy;

public abstract class ConnectionStateBase
{
    protected readonly object stateLock = new();

    protected bool connected;

    public Socket UpStreamSocket { get; }
    public Socket DownStreamSocket { get; }

    public byte[] UpStreamBuffer { get; }
    public byte[] DownStreamBuffer { get; }

    public EndPoint UpStreamEndPoint { get; }
    public EndPoint DownStreamEndPoint { get; }

    public Target Target { get; }

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

    protected ConnectionStateBase(
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
