using System.Collections.Generic;
using System.Linq;

using Zixoan.Cuber.Server.Config;

namespace Zixoan.Cuber.Server.Balancing
{
    public class LeastConnectionLoadBalanceStrategy : LoadBalanceStrategyBase
    {
        public LeastConnectionLoadBalanceStrategy(List<Target> targets)
            : base(targets)
        {
        }

        public override Target GetTarget()
            => this.targets.Aggregate((min, other) => other.Connections < min.Connections ? other : min);

        public Target GetTarget2()
            => this.targets.Select(target => (target.Connections, target)).Min().target;
    }
}
