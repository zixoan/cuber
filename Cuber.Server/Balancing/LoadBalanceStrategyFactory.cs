using Zixoan.Cuber.Server.Config;
using Zixoan.Cuber.Server.Provider;

namespace Zixoan.Cuber.Server.Balancing
{
    public class LoadBalanceStrategyFactory
    {
        public static ILoadBalanceStrategy Create(BalanceStrategy balanceStrategy, ITargetProvider targetProvider)
        {
            return balanceStrategy switch
            {
                BalanceStrategy.Random => new RandomLoadBalanceStrategy(targetProvider),
                _ => new RoundRobinLoadBalanceStrategy(targetProvider)
            };
        }
    }
}
