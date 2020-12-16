using Zixoan.Cuber.Server.Config;
using Zixoan.Cuber.Server.Provider;

namespace Zixoan.Cuber.Server.Balancing
{
    public class LeastConnectionLoadBalanceStrategy : LoadBalanceStrategyBase
    {
        public LeastConnectionLoadBalanceStrategy(ITargetProvider targetProvider)
            : base(targetProvider)
        {
        }

        public override Target GetTarget()
            => this.targetProvider.Aggregate((min, other) => other.Connections < min.Connections ? other : min);
    }
}
