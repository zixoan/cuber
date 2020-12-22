using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Xunit;

using Zixoan.Cuber.Server.Balancing;
using Zixoan.Cuber.Server.Config;
using Zixoan.Cuber.Server.Provider;
using Zixoan.Cuber.Server.Proxy;
using Zixoan.Cuber.Server.Proxy.Tcp;
using Zixoan.Cuber.Server.Proxy.Udp;

namespace Zixoan.Cuber.Server.Tests
{
    public class ProxyTests
    {
        [Fact]
        public async Task TcpProxyConnectAndEcho()
        {
            ushort proxyServerPort = PortHelper.GetFreePort();

            byte[] expectedBufferTarget = Encoding.UTF8.GetBytes("Hello, target!");
            byte[] expectedBufferClient = Encoding.UTF8.GetBytes("Hello, client!");
            byte[] actualBuffer = new byte[expectedBufferTarget.Length];

            TcpListener targetServer = new TcpListener(IPAddress.Loopback, 0);
            targetServer.Start();

            List<Target> targets = new List<Target>
            {
                new Target
                { 
                    Ip = IPAddress.Loopback.ToString(), 
                    Port = ((IPEndPoint)targetServer.LocalEndpoint).Port 
                }
            };
            ITargetProvider targetProvider = new SimpleTargetProvider(targets);

            IProxy tcpProxy = new TcpProxy(new NullLogger<TcpProxy>(), Options.Create(new CuberOptions()), new RoundRobinLoadBalanceStrategy(targetProvider));
            tcpProxy.Listen(IPAddress.Loopback.ToString(), proxyServerPort);

            Assert.Equal(0, targets[0].Connections);

            // Start async accept of the tcp proxy connection
            // that will happen below
            Task<Socket> waitingAccept = targetServer.AcceptSocketAsync();

            // Connect client to tcp proxy, 
            // which will then connect to the target server
            using (TcpClient tcpClient = new TcpClient())
            {
                tcpClient.Connect(IPAddress.Loopback, proxyServerPort);

                using Socket acceptedClientOnTarget = await waitingAccept;

                // Give the async target connect some time to complete
                await Task.Delay(25);

                Assert.Equal(1, targets[0].Connections);

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

            Assert.Equal(0, targets[0].Connections);

            tcpProxy.Stop();
        }

        [Fact]
        public async Task UdpProxyConnectAndEcho()
        {
            ushort proxyServerPort = PortHelper.GetFreePort();

            byte[] expectedBufferTarget = Encoding.UTF8.GetBytes("Hello, target!");
            byte[] expectedBufferClient = Encoding.UTF8.GetBytes("Hello, client!");

            UdpClient targetServer = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));

            List<Target> targets = new List<Target>
            {
                new Target
                { 
                    Ip = IPAddress.Loopback.ToString(), 
                    Port = ((IPEndPoint)targetServer.Client.LocalEndPoint).Port 
                }
            };
            ITargetProvider targetProvider = new SimpleTargetProvider(targets);

            IProxy udpProxy = new UdpProxy(new NullLogger<UdpProxy>(), Options.Create(new CuberOptions()), new RoundRobinLoadBalanceStrategy(targetProvider));
            udpProxy.Listen(IPAddress.Loopback.ToString(), proxyServerPort);

            using (UdpClient udpClient = new UdpClient())
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
}
