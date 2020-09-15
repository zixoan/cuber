using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Zixoan.Cuber.Server.Balancing;
using Zixoan.Cuber.Server.Config;

namespace Zixoan.Cuber.Server
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var hostBuilder = new HostBuilder()
                .ConfigureAppConfiguration((hostContext, configApp) =>
                {
                    configApp.SetBasePath(Directory.GetCurrentDirectory());
                    configApp.AddJsonFile("cuber.json", optional: true);
                    configApp.AddEnvironmentVariables(prefix: "CUBER_");
                    configApp.AddCommandLine(args);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddLogging(logBuilder =>
                    {
                        logBuilder
                            .AddConsole()
                            .AddConfiguration(hostContext.Configuration.GetSection("Logging"));
                    });
                    services.Configure<CuberOptions>(hostContext.Configuration.GetSection("Cuber"));
                    services.AddSingleton(serviceProvider =>
                    {
                        CuberOptions options = hostContext.Configuration.GetSection("Cuber").Get<CuberOptions>();
                        return LoadBalanceStrategyFactory.Create(options.BalanceStrategy, options.Targets);
                    });
                    services.AddHostedService<CuberHostedService>();
                });

            await hostBuilder.RunConsoleAsync();
        }
    }
}
