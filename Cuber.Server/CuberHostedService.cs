using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Zixoan.Cuber.Server.Config;

namespace Zixoan.Cuber.Server
{
    public class CuberHostedService : IHostedService
    {
        private readonly ILogger logger;
        private readonly CuberOptions cuberOptions;

        public CuberHostedService(ILogger<CuberHostedService> logger, IOptions<CuberOptions> cuberOptions)
        {
            this.logger = logger;
            this.cuberOptions = cuberOptions.Value;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Starting Cuber server");
            this.logger.LogInformation($"Using load balance strategy {this.cuberOptions.BalanceStrategy} with {this.cuberOptions.Targets.Count} targets");

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Stopping Cuber service");

            return Task.CompletedTask;
        }
    }
}
