using System.Collections.Generic;
using System.Net;

using Xunit;

using Zixoan.Cuber.Server.Balancing;
using Zixoan.Cuber.Server.Config;
using Zixoan.Cuber.Server.Extensions;
using Zixoan.Cuber.Server.Provider;

namespace Zixoan.Cuber.Server.Tests
{
    public class LoadBalanceStrategyTests
    {
        private static readonly List<Target> Targets = new List<Target>
            {
                new Target { Ip = "10.0.0.1", Port = 8080 },
                new Target { Ip = "10.0.0.2", Port = 8081 },
                new Target { Ip = "10.0.0.3", Port = 8082 }
            };

        private static readonly ITargetProvider TargetProvider = new SimpleTargetProvider(Targets);

        [Fact]
        public void TestRoundRobinStrategy()
        {
            ILoadBalanceStrategy roundRobin = LoadBalanceStrategyFactory.Create(BalanceStrategy.RoundRobin, TargetProvider);

            for (int i = 0; i < 26; i++)
            {
                Assert.Equal(Targets[i % Targets.Count], roundRobin.GetTarget(null));
            }
        }

        [Fact]
        public void TestRandomStrategy()
        {
            ILoadBalanceStrategy random = LoadBalanceStrategyFactory.Create(BalanceStrategy.Random, TargetProvider);

            Assert.Contains(random.GetTarget(null), Targets);
            Assert.Contains(random.GetTarget(null), Targets);
            Assert.Contains(random.GetTarget(null), Targets);
            Assert.Contains(random.GetTarget(null), Targets);
        }

        [Fact]
        public void TestLeastConnectionStrategy()
        {
            ILoadBalanceStrategy leastConnection = new LeastConnectionLoadBalanceStrategy(TargetProvider);

            Targets[1].IncrementConnections();
            Targets[1].IncrementConnections();

            Assert.Equal(Targets[0], leastConnection.GetTarget(null));

            Targets[0].IncrementConnections();
            Targets[0].IncrementConnections();
            Targets[0].IncrementConnections();

            Assert.Equal(Targets[2], leastConnection.GetTarget(null));

            Targets[1].IncrementConnections();

            Targets[2].IncrementConnections();
            Targets[2].IncrementConnections();
            Targets[2].IncrementConnections();

            Assert.Equal(Targets[0], leastConnection.GetTarget(null));

            Targets[0].IncrementConnections();

            Assert.Equal(Targets[1], leastConnection.GetTarget(null));
        }

        [Fact]
        public void TestHashStrategy()
        {
            ILoadBalanceStrategy hash = new HashLoadBalanceStrategy(TargetProvider);

            IPAddress ip1 = IPAddress.Parse("10.1.1.1");
            int targetIndex1 = ip1.GetHashCode().FloorMod(Targets.Count);

            IPAddress ip2 = IPAddress.Parse("150.181.45.241");
            int targetIndex2 = ip2.GetHashCode().FloorMod(Targets.Count);

            for (int i = 0; i < 4; i++)
            {
                Assert.Equal(Targets[targetIndex1], hash.GetTarget(new IPEndPoint(ip1, 0)));
                Assert.Equal(Targets[targetIndex2], hash.GetTarget(new IPEndPoint(ip2, 0)));
            }
        }
    }
}
