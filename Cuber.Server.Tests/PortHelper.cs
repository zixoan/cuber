using System.Net;
using System.Net.Sockets;

namespace Zixoan.Cuber.Server.Tests;

public static class PortHelper
{
    public static ushort GetFreePort()
    {
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        return (ushort)(((IPEndPoint)socket.LocalEndPoint!)!).Port;
    }
}