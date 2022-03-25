using Zixoan.Cuber.Server.Balancing;

namespace Zixoan.Cuber.Server.Proxy;

public abstract class ProxyBase : IProxy
{
    protected readonly ILoadBalanceStrategy loadBalanceStrategy;

    protected ProxyBase(ILoadBalanceStrategy loadBalanceStrategy)
        => this.loadBalanceStrategy = loadBalanceStrategy; 

    public abstract void Start();
    public abstract void Stop();
}