using System.Collections.Generic;

using Xunit;

using Zixoan.Cuber.Server.Balancing;
using Zixoan.Cuber.Server.Config;
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
                Assert.Equal(Targets[i % Targets.Count], roundRobin.GetTarget());
            }
        }

        [Fact]
        public void TestRandomStrategy()
        {
            ILoadBalanceStrategy random = LoadBalanceStrategyFactory.Create(BalanceStrategy.Random, TargetProvider);

            Assert.Contains(random.GetTarget(), Targets);
            Assert.Contains(random.GetTarget(), Targets);
            Assert.Contains(random.GetTarget(), Targets);
            Assert.Contains(random.GetTarget(), Targets);
        }

        [Fact]
        public void TestLeastConnectionStrategy()
        {
            ILoadBalanceStrategy leastConnection = new LeastConnectionLoadBalanceStrategy(TargetProvider);

            Targets[1].IncrementConnections();
            Targets[1].IncrementConnections();

            Assert.Equal(Targets[0], leastConnection.GetTarget());

            Targets[0].IncrementConnections();
            Targets[0].IncrementConnections();
            Targets[0].IncrementConnections();

            Assert.Equal(Targets[2], leastConnection.GetTarget());

            Targets[1].IncrementConnections();

            Targets[2].IncrementConnections();
            Targets[2].IncrementConnections();
            Targets[2].IncrementConnections();

            Assert.Equal(Targets[0], leastConnection.GetTarget());

            Targets[0].IncrementConnections();

            Assert.Equal(Targets[1], leastConnection.GetTarget());
        }
    }
}
