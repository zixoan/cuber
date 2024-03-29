﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Zixoan.Cuber.Server.Config;
using Zixoan.Cuber.Server.Extensions;
using Zixoan.Cuber.Server.Web;

namespace Zixoan.Cuber.Server;

public class Program
{
    public static async Task Main(string[] args)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("cuber.json", optional: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();

        var cuberOptions = new CuberOptions();
        configuration.GetSection("Cuber").Bind(cuberOptions);

        IHostBuilder hostBuilder = CreateConsoleHostBuilder(configuration, cuberOptions.Web.Urls);

        await hostBuilder.RunConsoleAsync();
    }

    private static IHostBuilder CreateConsoleHostBuilder(
        IConfiguration configuration,
        string[] urls)
    {
        IHostBuilder hostBuilder = new HostBuilder()
            .ConfigureAppConfiguration((_, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddConfiguration(configuration);
            })
            .ConfigureServices((_, services) =>
            {
                services.AddLogging(logBuilder =>
                {
                    logBuilder
                        .AddConsole()
                        .AddConfiguration(configuration.GetSection("Logging"));
                });
                services.AddCuber(configuration);
            })
            .ConfigureWebHost(webConfig =>
            {
                webConfig.UseKestrel();
                webConfig.UseStartup<CuberWebStartup>();
                webConfig.UseUrls(urls);
                webConfig.ConfigureLogging(logging =>
                {
                    logging
                        .AddConsole()
                        .AddConfiguration(configuration.GetSection("Logging"));
                });
            })
            .UseConsoleLifetime();
        return hostBuilder;
    }
}
