using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using NSubstitute;

using Xunit;

using Zixoan.Cuber.Server.Balancing;
using Zixoan.Cuber.Server.Config;
using Zixoan.Cuber.Server.Proxy;
using Zixoan.Cuber.Server.Proxy.Tcp;
using Zixoan.Cuber.Server.Proxy.Udp;
using Zixoan.Cuber.Server.Stats;

namespace Zixoan.Cuber.Server.Tests;

public class ProxyTests
{
    private readonly IStatsService statsService;

    public ProxyTests()
        => this.statsService = Substitute.For<IStatsService>();

    [Fact]
    public async Task TcpProxyConnectAndEcho()
    {
        ushort proxyServerPort = PortHelper.GetFreePort();

        byte[] expectedBufferTarget = Encoding.UTF8.GetBytes("Hello, target!");
        byte[] expectedBufferClient = Encoding.UTF8.GetBytes("Hello, client!");
        byte[] actualBuffer = new byte[expectedBufferTarget.Length];

        var targetServer = new TcpListener(IPAddress.Loopback, 0);
        targetServer.Start();

        var target = new Target(IPAddress.Loopback.ToString(), (ushort)((IPEndPoint)targetServer.LocalEndpoint).Port);
        ILoadBalanceStrategy loadBalanceStrategy = Substitute.For<ILoadBalanceStrategy>();
        loadBalanceStrategy.GetTarget(Arg.Any<IPEndPoint>())
            .Returns(target);

        IProxy tcpProxy = new TcpProxy(new NullLogger<TcpProxy>(), Options.Create(new CuberOptions { Ip = IPAddress.Loopback.ToString(), Port = proxyServerPort }), statsService, loadBalanceStrategy);
        tcpProxy.Start();

        Assert.Equal(0, target.Connections);

        // Start async accept of the tcp proxy connection
        // that will happen below
        Task<Socket> waitingAccept = targetServer.AcceptSocketAsync();

        // Connect client to tcp proxy, 
        // which will then connect to the target server
        using (var tcpClient = new TcpClient())
        {
            // Connect synchronous so we can await the accept task
            // after the connect is finished
            tcpClient.Connect(IPAddress.Loopback, proxyServerPort);

            using Socket acceptedClientOnTarget = await waitingAccept;

            // Give the async target connect some time to complete
            await Task.Delay(25);

            Assert.Equal(1, target.Connections);

            // Client -> Proxy -> Target
            await tcpClient.GetStream().WriteAsync(expectedBufferTarget);
            await acceptedClientOnTarget.ReceiveAsync(actualBuffer, SocketFlags.None);
            Assert.Equal(expectedBufferTarget, actualBuffer);

            // Target -> Proxy -> Client
            await acceptedClientOnTarget.SendAsync(expectedBufferClient, SocketFlags.None);
            await tcpClient.GetStream().ReadAsync(actualBuffer);
            Assert.Equal(expectedBufferClient, actualBuffer);
        }

        targetServer.Stop();

        // Give the proxy connection some time to disconnect properly
        await Task.Delay(25);

        Assert.Equal(0, target.Connections);

        tcpProxy.Stop();
    }

    [Fact]
    public async Task UdpProxyConnectAndEcho()
    {
        ushort proxyServerPort = PortHelper.GetFreePort();

        byte[] expectedBufferTarget = Encoding.UTF8.GetBytes("Hello, target!");
        byte[] expectedBufferClient = Encoding.UTF8.GetBytes("Hello, client!");

        var targetServer = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));

        var target = new Target(IPAddress.Loopback.ToString(), (ushort)(((IPEndPoint)targetServer.Client.LocalEndPoint!)!).Port);
        ILoadBalanceStrategy loadBalanceStrategy = Substitute.For<ILoadBalanceStrategy>();
        loadBalanceStrategy.GetTarget(Arg.Any<IPEndPoint>())
            .Returns(target);
            
        IProxy udpProxy = new UdpProxy(new NullLogger<UdpProxy>(), Options.Create(new CuberOptions { Ip = IPAddress.Loopback.ToString(), Port = proxyServerPort }), statsService, loadBalanceStrategy);
        udpProxy.Start();

        using (var udpClient = new UdpClient())
        {
            udpClient.Connect(IPAddress.Loopback, proxyServerPort);
                
            // Client -> Proxy -> Target
            await udpClient.SendAsync(expectedBufferTarget, expectedBufferTarget.Length);
            UdpReceiveResult udpReceiveResultTarget = await targetServer.ReceiveAsync();
            Assert.Equal(expectedBufferTarget, udpReceiveResultTarget.Buffer);

            // Target -> Proxy -> Client
            await targetServer.SendAsync(expectedBufferClient, expectedBufferClient.Length, udpReceiveResultTarget.RemoteEndPoint);
            UdpReceiveResult udpReceiveResultClient = await udpClient.ReceiveAsync();
            Assert.Equal(expectedBufferClient, udpReceiveResultClient.Buffer);
        }

        targetServer.Close();
        udpProxy.Stop();
    }
}