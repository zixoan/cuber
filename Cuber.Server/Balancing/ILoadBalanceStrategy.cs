using Zixoan.Cuber.Server.Config;

namespace Zixoan.Cuber.Server.Balancing
{
    public interface ILoadBalanceStrategy
    {
        public Target GetTarget();
    }
}
