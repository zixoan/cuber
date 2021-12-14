using System;
using System.Net;

using Zixoan.Cuber.Server.Config;
using Zixoan.Cuber.Server.Extensions;
using Zixoan.Cuber.Server.Provider;

namespace Zixoan.Cuber.Server.Balancing
{
    public class HashLoadBalanceStrategy : LoadBalanceStrategyBase
    {
        public HashLoadBalanceStrategy(ITargetProvider targetProvider)
            : base(targetProvider)
        {

        }

        public override Target? GetTarget(EndPoint? sourceEndPoint)
        {
            int targetCount = this.targetProvider.Count;
            if (targetCount == 0)
            {
                return null;
            }

            if (sourceEndPoint is IPEndPoint ipEndPoint)
            {
                int sourceIpHashCode = ipEndPoint.Address.GetHashCode();
                int targetIndex = sourceIpHashCode.FloorMod(targetCount);

                return this.targetProvider[targetIndex];
            }

            throw new NotSupportedException("sourceEndPoint needs to be an IPEndPoint");
        }
    }
}
