using Zixoan.Cuber.Server.Config;
using Zixoan.Cuber.Server.Provider;

namespace Zixoan.Cuber.Server.Balancing
{
    public class RoundRobinLoadBalanceStrategy : LoadBalanceStrategyBase
    {
        private int currentTarget;

        public RoundRobinLoadBalanceStrategy(ITargetProvider targetProvider) 
            : base(targetProvider)
        {
        }

        public override Target GetTarget()
            => this.targetProvider[currentTarget++ % this.targetProvider.Count];
    }
}
