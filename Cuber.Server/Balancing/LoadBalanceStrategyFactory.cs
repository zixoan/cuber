using Zixoan.Cuber.Server.Config;
using Zixoan.Cuber.Server.Provider;

namespace Zixoan.Cuber.Server.Balancing
{
    public static class LoadBalanceStrategyFactory
    {
        public static ILoadBalanceStrategy Create(BalanceStrategy balanceStrategy, ITargetProvider targetProvider)
        {
            return balanceStrategy switch
            {
                BalanceStrategy.Random => new RandomLoadBalanceStrategy(targetProvider),
                BalanceStrategy.LeastConnection => new LeastConnectionLoadBalanceStrategy(targetProvider),
                BalanceStrategy.Hash => new HashLoadBalanceStrategy(targetProvider),
                _ => new RoundRobinLoadBalanceStrategy(targetProvider)
            };
        }
    }
}
