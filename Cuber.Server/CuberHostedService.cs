using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Zixoan.Cuber.Server.Config;
using Zixoan.Cuber.Server.Proxy;

namespace Zixoan.Cuber.Server;

public class CuberHostedService : IHostedService
{
    private readonly ILogger logger;
    private readonly CuberOptions cuberOptions;
    private readonly IProxy proxy;

    public CuberHostedService(
        ILogger<CuberHostedService> logger, 
        IOptions<CuberOptions> cuberOptions,
        IProxy proxy)
    {
        this.logger = logger;
        this.cuberOptions = cuberOptions.Value;
        this.proxy = proxy;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation("Starting Cuber server");
        this.logger.LogInformation("Using load balance strategy {balanceStrategy} with {targetCount} targets",
            this.cuberOptions.BalanceStrategy, this.cuberOptions.Targets?.Count() ?? 0);

        this.proxy.Start();

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        this.proxy.Stop();

        this.logger.LogInformation("Stopped Cuber service");

        return Task.CompletedTask;
    }
}
