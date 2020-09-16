using Zixoan.Cuber.Server.Balancing;

namespace Zixoan.Cuber.Server.Proxy
{
    public abstract class ProxyBase : IProxy
    {
        protected ILoadBalanceStrategy loadBalanceStrategy;

        public ProxyBase(ILoadBalanceStrategy loadBalanceStrategy)
        {
            this.loadBalanceStrategy = loadBalanceStrategy;
        }

        public abstract void Listen(string ip, ushort port);
        public abstract void Stop();
    }
}
