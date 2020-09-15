using System.Collections.Generic;

using Zixoan.Cuber.Server.Config;

namespace Zixoan.Cuber.Server.Balancing
{
    public class LoadBalanceStrategyFactory
    {
        public static ILoadBalanceStrategy Create(BalanceStrategy balanceStrategy, List<Target> targets)
        {
            return balanceStrategy switch
            {
                BalanceStrategy.Random => new RandomLoadBalanceStrategy(targets),
                _ => new RoundRobinLoadBalanceStrategy(targets)
            };
        }
    }
}
