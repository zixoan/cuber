using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using Xunit;

using Zixoan.Cuber.Server.Config;
using Zixoan.Cuber.Server.Probe;

namespace Zixoan.Cuber.Server.Tests
{
    public class HealthProbeTests
    {
        [Fact]
        public async Task TcpHealthProbeSuccessful()
        {
            IHealthProbe tcpProbe = new TcpHealthProbe();

            TcpListener tcpListener = new TcpListener(IPAddress.Loopback, 0);
            tcpListener.Start();

            Target target = new Target { Ip = IPAddress.Loopback.ToString(), Port = ((IPEndPoint)tcpListener.LocalEndpoint).Port };

            Task<bool> reachableTask = tcpProbe.IsReachable(target);

            await tcpListener.AcceptSocketAsync();

            Assert.True(await reachableTask);
        }

        [Fact]
        public async Task TcpHealthProbeUnsuccessful()
        {
            IHealthProbe tcpProbe = new TcpHealthProbe();

            Target target = new Target { Ip = IPAddress.Loopback.ToString(), Port = 0 };

            Assert.False(await tcpProbe.IsReachable(target));
        }
    }
}
