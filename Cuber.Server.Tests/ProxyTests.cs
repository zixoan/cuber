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
using Zixoan.Cuber.Server.Proxy;
using Zixoan.Cuber.Server.Proxy.Tcp;

namespace Zixoan.Cuber.Server.Tests
{
    public class ProxyTests
    {
        private static ushort GetServerPort()
        {
            using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
            return (ushort)((IPEndPoint)socket.LocalEndPoint).Port;
        }

        [Fact]
        public async Task TcpProxyConnectAndEcho()
        {
            ushort proxyServerPort = GetServerPort();

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

            IProxy tcpProxy = new TcpProxy(new NullLogger<TcpProxy>(), Options.Create(new CuberOptions()), new RoundRobinLoadBalanceStrategy(targets));
            tcpProxy.Listen(IPAddress.Loopback.ToString(), proxyServerPort);

            // Start async accept of the tcp proxy connection
            // that will happen below
            Task<Socket> waitingAccept = targetServer.AcceptSocketAsync();

            // Connect client to tcp proxy, 
            // which will then connect to the target server
            using TcpClient tcpClient = new TcpClient();
            tcpClient.Connect(IPAddress.Loopback, proxyServerPort);

            Socket acceptedClientOnTarget = await waitingAccept;

            // Client -> Proxy -> Target
            await tcpClient.GetStream().WriteAsync(expectedBufferTarget);
            await acceptedClientOnTarget.ReceiveAsync(actualBuffer, SocketFlags.None);
            Assert.Equal(expectedBufferTarget, actualBuffer);

            // Target -> Proxy -> Client
            await acceptedClientOnTarget.SendAsync(expectedBufferClient, SocketFlags.None);
            await tcpClient.GetStream().ReadAsync(actualBuffer);
            Assert.Equal(expectedBufferClient, actualBuffer);

            targetServer.Stop();
        }
    }
}
