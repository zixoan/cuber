using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Zixoan.Cuber.Server.Config;

namespace Zixoan.Cuber.Server.Extensions;

public static class CuberServiceCollectionExtensions
{
    public static IServiceCollection AddCuber(this IServiceCollection @this, IConfiguration configuration)
    {
        @this.Configure<CuberOptions>(configuration.GetSection("Cuber"));

        @this
            .AddTargetProvider()
            .AddLoadBalancing()
            .AddHealthProbe()
            .AddProxy()
            .AddStats();

        @this.AddHostedService<CuberHostedService>();
        return @this;
    }
}
