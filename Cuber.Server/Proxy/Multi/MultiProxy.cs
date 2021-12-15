using System.Collections.Generic;

using Microsoft.Extensions.Logging;

using Zixoan.Cuber.Server.Balancing;

namespace Zixoan.Cuber.Server.Proxy.Multi
{
    public class MultiProxy : ProxyBase
    {
        private readonly ILogger<MultiProxy> logger;
        private readonly IReadOnlyList<IProxy> proxies;

        public MultiProxy(
            ILogger<MultiProxy> logger,
            IReadOnlyList<IProxy> proxies,
            ILoadBalanceStrategy loadBalanceStrategy)
            : base(loadBalanceStrategy)
        {
            this.logger = logger;
            this.proxies = proxies;
        }

        public override void Start()
        {
            foreach (IProxy proxy in this.proxies)
            {
                proxy.Start();
            }

            this.logger.LogInformation("Multi proxy started");
        }

        public override void Stop()
        {
            foreach (IProxy proxy in this.proxies)
            {
                proxy.Stop();
            }

            this.logger.LogInformation("Multi proxy stopped");
        }
    }
}
