using System.Net;

using Zixoan.Cuber.Server.Config;
using Zixoan.Cuber.Server.Provider;

namespace Zixoan.Cuber.Server.Balancing;

public class RandomLoadBalanceStrategy : LoadBalanceStrategyBase
{
    private readonly Random random;

    public RandomLoadBalanceStrategy(ITargetProvider targetProvider)
        : base(targetProvider)
    {
        this.random = new Random();
    }

    public override Target? GetTarget(EndPoint? sourceEndPoint)
    {
        int count = this.targetProvider.Count;
        return count == 0
            ? null
            : this.targetProvider[this.random.Next(count)];
    }
}