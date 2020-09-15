using System.Collections.Generic;

using Zixoan.Cuber.Server.Config;

namespace Zixoan.Cuber.Server.Balancing
{
    public class RoundRobinLoadBalanceStrategy : LoadBalanceStrategyBase
    {
        private int currentTarget;

        public RoundRobinLoadBalanceStrategy(List<Target> targets) 
            : base(targets)
        {
        }

        public override Target GetTarget()
            => this.targets[currentTarget++ % this.targets.Count];
    }
}
