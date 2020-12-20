using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Zixoan.Cuber.Server.Balancing;
using Zixoan.Cuber.Server.Config;
using Zixoan.Cuber.Server.Probe;
using Zixoan.Cuber.Server.Provider;
using Zixoan.Cuber.Server.Proxy;
using Zixoan.Cuber.Server.Proxy.Tcp;

namespace Zixoan.Cuber.Server
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            IHostBuilder hostBuilder = new HostBuilder()
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
                    services.AddSingleton<ITargetProvider, ThreadSafeTargetProvider>(serviceProvider =>
                    {
                        CuberOptions options = hostContext.Configuration.GetSection("Cuber").Get<CuberOptions>();
                        return new ThreadSafeTargetProvider(options.Targets); 
                    });
                    services.AddSingleton(serviceProvider =>
                    {
                        CuberOptions options = hostContext.Configuration.GetSection("Cuber").Get<CuberOptions>();
                        ITargetProvider targetProvider = serviceProvider.GetService<ITargetProvider>();
                        return LoadBalanceStrategyFactory.Create(options.BalanceStrategy, targetProvider);
                    });
                    services.AddSingleton<IProxy, ProxyBase>(serviceProvider =>
                    {
                        CuberOptions options = hostContext.Configuration.GetSection("Cuber").Get<CuberOptions>();
                        
                        ILoggerFactory loggerFactory = serviceProvider.GetService<ILoggerFactory>();
                        ILoadBalanceStrategy loadBalanceStrategy = serviceProvider.GetService<ILoadBalanceStrategy>();

                        ProxyBase proxy = options.Mode switch
                        {
                            Mode.Udp => throw new NotImplementedException(),
                            _ => new TcpProxy(loggerFactory.CreateLogger<TcpProxy>(), Options.Create(options), loadBalanceStrategy),
                        };
                        return proxy;
                    });
                    services.AddHostedService<CuberHostedService>();
                    
                    CuberOptions options = hostContext.Configuration.GetSection("Cuber").Get<CuberOptions>();
                    if (options.HealthProbe != null)
                    {
                        HealthProbeType? type = options.HealthProbe.Type;
                        if (type == null)
                        {
                            throw new ArgumentException("At least Type must be defined when HealthProbe is defined in config");
                        }

                        services.AddSingleton<IHealthProbe>(serviceProvider => HealthProbeFactory.Create(type.Value, options));
                        services.AddHostedService<HealthProbeBackgroundService>();
                    }
                });

            await hostBuilder.RunConsoleAsync();
        }
    }
}
