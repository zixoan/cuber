using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Zixoan.Cuber.Server.Config;
using Zixoan.Cuber.Server.Provider;

namespace Zixoan.Cuber.Server.Probe
{
    public class HealthProbeBackgroundService : BackgroundService, IDisposable
    {
        private readonly ILogger<HealthProbeBackgroundService> logger;
        private readonly ITargetProvider targetProvider;
        private readonly IHealthProbe healthProbe;
        private readonly CuberOptions cuberOptions;
        private readonly IList<Target> offlineTargets;

        public HealthProbeBackgroundService(
            ILogger<HealthProbeBackgroundService> logger, 
            ITargetProvider targetProvider,
            IHealthProbe healthProbe,
            IOptions<CuberOptions> cuberOptions)
        {
            this.logger = logger;
            this.targetProvider = targetProvider;
            this.healthProbe = healthProbe;
            this.cuberOptions = cuberOptions.Value;
            this.offlineTargets = new List<Target>();
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation($"Starting {this.cuberOptions.HealthProbe?.Type} health probe");
            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                IList<Target> targets = this.targetProvider.Targets;

                for (int i = 0; i < targets.Count; i++)
                {
                    Target target = targets[i];

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

                await Task.Delay(this.cuberOptions.HealthProbe.Interval, cancellationToken);
            }
        }
    }
}
