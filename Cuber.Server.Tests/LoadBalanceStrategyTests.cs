using System.Collections.Generic;

using Xunit;

using Zixoan.Cuber.Server.Balancing;
using Zixoan.Cuber.Server.Config;

namespace Zixoan.Cuber.Server.Tests
{
    public class LoadBalanceStrategyTests
    {
        private static readonly List<Target> targets = new List<Target>
            {
                new Target { Ip = "10.0.0.1", Port = 8080 },
                new Target { Ip = "10.0.0.2", Port = 8081 },
                new Target { Ip = "10.0.0.3", Port = 8082 }
            };

        [Fact]
        public void TestRoundRobinStrategy()
        {
            var roundRobin = LoadBalanceStrategyFactory.Create(BalanceStrategy.RoundRobin, targets);

            Assert.Equal(targets[0], roundRobin.GetTarget());
            Assert.Equal(targets[1], roundRobin.GetTarget());
            Assert.Equal(targets[2], roundRobin.GetTarget());
            Assert.Equal(targets[0], roundRobin.GetTarget());
            Assert.Equal(targets[1], roundRobin.GetTarget());
            Assert.Equal(targets[2], roundRobin.GetTarget());
            Assert.Equal(targets[0], roundRobin.GetTarget());
        }

        [Fact]
        public void TestRandomStrategy()
        {
            var random = LoadBalanceStrategyFactory.Create(BalanceStrategy.Random, targets);

            Assert.Contains(random.GetTarget(), targets);
            Assert.Contains(random.GetTarget(), targets);
            Assert.Contains(random.GetTarget(), targets);
            Assert.Contains(random.GetTarget(), targets);
        }

        [Fact]
        public void TestLeastConnectionStrategy()
        {
            var leastConnection = new LeastConnectionLoadBalanceStrategy(targets);

            targets[1].IncrementConnections();
            targets[1].IncrementConnections();

            Assert.Equal(targets[0], leastConnection.GetTarget());

            targets[0].IncrementConnections();
            targets[0].IncrementConnections();
            targets[0].IncrementConnections();

            Assert.Equal(targets[2], leastConnection.GetTarget());

            targets[1].IncrementConnections();

            targets[2].IncrementConnections();
            targets[2].IncrementConnections();
            targets[2].IncrementConnections();

            Assert.Equal(targets[0], leastConnection.GetTarget());

            targets[0].IncrementConnections();

            Assert.Equal(targets[1], leastConnection.GetTarget());
        }
    }
}
