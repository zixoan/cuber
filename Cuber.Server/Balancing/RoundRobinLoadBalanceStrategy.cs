using System.Net;

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

        public override Target? GetTarget(EndPoint? sourceEndPoint)
        {
            int count = this.targetProvider.Count;
            if (count == 0)
            {
                return null;
            }

            return this.targetProvider[currentTarget++ % count];
        }
    }
}
