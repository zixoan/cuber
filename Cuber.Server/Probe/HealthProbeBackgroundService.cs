using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Zixoan.Cuber.Server.Config;
using Zixoan.Cuber.Server.Provider;

namespace Zixoan.Cuber.Server.Probe
{
    public class HealthProbeBackgroundService : BackgroundService, IDisposable
    {
        private readonly ILogger<HealthProbeBackgroundService> logger;
        private readonly ITargetProvider targetProvider;
        private readonly IHealthProbe healthProbe;
        private readonly IList<Target> initialTargets;
        private readonly IList<Target> offlineTargets;

        public HealthProbeBackgroundService(
            ILogger<HealthProbeBackgroundService> logger, 
            ITargetProvider targetProvider,
            IHealthProbe healthProbe)
        {
            this.logger = logger;
            this.targetProvider = targetProvider;
            this.healthProbe = healthProbe;
            this.initialTargets = this.targetProvider.Targets;
            this.offlineTargets = new List<Target>();
        }

        protected override async Task ExecuteAsync(CancellationToken cancelToken)
        {
            while (!cancelToken.IsCancellationRequested)
            {
                for (int i = 0; i < this.initialTargets.Count; i++)
                {
                    Target target = this.initialTargets[i];

                    bool reachable = await this.healthProbe.IsReachable(target);
                    if (!reachable && !this.offlineTargets.Contains(target))
                    {
                        this.targetProvider.Remove(i);
                        this.offlineTargets.Add(target);

                        logger.LogWarning("Target {0} went offline", target);
                    }
                    else if (reachable && this.offlineTargets.Contains(target))
                    {
                        this.targetProvider.Add(target);
                        this.offlineTargets.Remove(target);

                        logger.LogInformation("Target {0} went online", target);
                    }
                }

                await Task.Delay(5000, cancelToken);
            }
        }
    }
}
