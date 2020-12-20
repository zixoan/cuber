using System.Net;
using System.Net.Sockets;

public static class PortHelper
{
    public static ushort GetFreePort()
    {
        using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        return (ushort)((IPEndPoint)socket.LocalEndPoint).Port;
    }
}
