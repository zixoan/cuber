using System.Net;

using Zixoan.Cuber.Server.Config;
using Zixoan.Cuber.Server.Provider;

namespace Zixoan.Cuber.Server.Balancing
{
    public abstract class LoadBalanceStrategyBase : ILoadBalanceStrategy
    {
        protected readonly ITargetProvider targetProvider;

        protected LoadBalanceStrategyBase(ITargetProvider targetProvider)
        {
            this.targetProvider = targetProvider;
        }

        public abstract Target GetTarget(EndPoint sourceEndPoint);
    }
}
