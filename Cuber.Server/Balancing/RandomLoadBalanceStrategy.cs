using System;
using System.Collections.Generic;

using Zixoan.Cuber.Server.Config;

namespace Zixoan.Cuber.Server.Balancing
{
    public class RandomLoadBalanceStrategy : LoadBalanceStrategyBase
    {
        private readonly Random random;

        public RandomLoadBalanceStrategy(List<Target> targets)
            : base(targets)
        {
            this.random = new Random();
        }

        public override Target GetTarget()
            => this.targets[this.random.Next(this.targets.Count)];
    }
}
