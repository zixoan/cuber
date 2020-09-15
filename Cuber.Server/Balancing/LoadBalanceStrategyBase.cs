using System.Collections.Generic;

using Zixoan.Cuber.Server.Config;

namespace Zixoan.Cuber.Server.Balancing
{
    public abstract class LoadBalanceStrategyBase : ILoadBalanceStrategy
    {
        protected readonly List<Target> targets;

        protected LoadBalanceStrategyBase(List<Target> targets)
        {
            this.targets = targets;
        }

        public abstract Target GetTarget();
    }
}
