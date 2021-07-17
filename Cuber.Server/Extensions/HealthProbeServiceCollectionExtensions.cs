using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Zixoan.Cuber.Server.Config;
using Zixoan.Cuber.Server.Probe;

namespace Zixoan.Cuber.Server.Extensions
{
    public static class HealthProbeServiceCollectionExtensions
    {
        public static IServiceCollection AddHealthProbe(this IServiceCollection @this)
        {
            ServiceProvider serviceProvider = @this.BuildServiceProvider();

            IOptions<CuberOptions> options = serviceProvider.GetRequiredService<IOptions<CuberOptions>>();
            if (options.Value.HealthProbe != null)
            {
                HealthProbeType? type = options.Value.HealthProbe.Type;
                if (type == null)
                {
                    throw new ArgumentException("At least Type must be defined when HealthProbe is defined in config");
                }

                @this.AddSingleton<IHealthProbe>(serviceProvider => HealthProbeFactory.Create(type.Value, options.Value));
                @this.AddHostedService<HealthProbeBackgroundService>();
            }
            return @this;
        }
    }
}
