using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Zixoan.Cuber.Server.Config;
using Zixoan.Cuber.Server.Extensions;

namespace Zixoan.Cuber.Server
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            IHostBuilder hostBuilder = new HostBuilder()
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("cuber.json", optional: true);
                    config.AddEnvironmentVariables(prefix: "CUBER_");
                    config.AddCommandLine(args);
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

                    services
                        .AddTargetProvider()
                        .AddLoadBalancing()
                        .AddHealthProbe()
                        .AddProxy();

                    services.AddHostedService<CuberHostedService>();
                });

            await hostBuilder.RunConsoleAsync();
        }
    }
}
