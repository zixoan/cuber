using System.Net;

using Zixoan.Cuber.Server.Config;
using Zixoan.Cuber.Server.Provider;

namespace Zixoan.Cuber.Server.Balancing;

public class LeastConnectionLoadBalanceStrategy : LoadBalanceStrategyBase
{
    public LeastConnectionLoadBalanceStrategy(ITargetProvider targetProvider)
        : base(targetProvider)
    {
    }

    public override Target? GetTarget(EndPoint? sourceEndPoint)
        => this.targetProvider.Aggregate((min, other) => other.Connections < min.Connections ? other : min);
}