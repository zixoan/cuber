using System.IO;
using System.Threading.Tasks;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Zixoan.Cuber.Server.Config;
using Zixoan.Cuber.Server.Extensions;
using Zixoan.Cuber.Server.Probe;
using Zixoan.Cuber.Server.Provider;
using Zixoan.Cuber.Server.Web;

namespace Zixoan.Cuber.Server
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("cuber.json", optional: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            CuberOptions cuberOptions = new CuberOptions();
            configuration.GetSection("Cuber").Bind(cuberOptions);

            ITargetProvider targetProvider = new ThreadSafeTargetProvider(cuberOptions.Targets);

            IHostBuilder hostBuilder = CreateConsoleHostBuilder(configuration, targetProvider);
            IWebHostBuilder webHostBuilder = CreateWebHostBuilder(configuration, cuberOptions, targetProvider);

            await Task.WhenAny(
                webHostBuilder.Build().RunAsync(),
                hostBuilder.RunConsoleAsync()
            );
        }

        private static IHostBuilder CreateConsoleHostBuilder(
            IConfiguration configuration,
            ITargetProvider targetProvider)
        {
            IHostBuilder hostBuilder = new HostBuilder()
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddConfiguration(configuration);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddLogging(logBuilder =>
                    {
                        logBuilder
                            .AddConsole()
                            .AddConfiguration(configuration.GetSection("Logging"));
                    });
                    services.Configure<CuberOptions>(configuration.GetSection("Cuber"));

                    services
                        .AddSingleton(targetProvider)
                        .AddLoadBalancing()
                        .AddHealthProbe()
                        .AddProxy();

                    services.AddHostedService<CuberHostedService>();
                });
            return hostBuilder;
        }

        private static IWebHostBuilder CreateWebHostBuilder(
            IConfiguration configuration,
            CuberOptions cuberOptions,
            ITargetProvider targetProvider)
        {
            IWebHostBuilder webHostBuilder = WebHost.CreateDefaultBuilder()
                .UseStartup<CuberWebStartup>()
                .UseUrls(cuberOptions.Web.Urls)
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddConfiguration(configuration);
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton(targetProvider);
                });
            return webHostBuilder;
        }
    }
}
