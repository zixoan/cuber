using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Zixoan.Cuber.Server.Config;
using Zixoan.Cuber.Server.Probe;

namespace Zixoan.Cuber.Server.Extensions;

public static class HealthProbeServiceCollectionExtensions
{
    public static IServiceCollection AddHealthProbe(this IServiceCollection @this)
    {
        ServiceProvider serviceProvider = @this.BuildServiceProvider();

        IOptions<CuberOptions> cuberOptions = serviceProvider.GetRequiredService<IOptions<CuberOptions>>();
        if (cuberOptions.Value.HealthProbe == null)
        {
            return @this;
        }

        HealthProbeType? type = cuberOptions.Value.HealthProbe.Type;
        if (type == null)
        {
            throw new ArgumentException("At least Type must be defined when HealthProbe is defined in config");
        }

        @this.AddSingleton(_ => HealthProbeFactory.Create(type.Value, cuberOptions));
        @this.AddHostedService<HealthProbeBackgroundService>();
        return @this;
    }
}