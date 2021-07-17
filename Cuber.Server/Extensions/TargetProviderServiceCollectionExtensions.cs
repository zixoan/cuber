using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Zixoan.Cuber.Server.Config;
using Zixoan.Cuber.Server.Probe;
using Zixoan.Cuber.Server.Provider;

namespace Zixoan.Cuber.Server.Extensions
{
    public static class TargetProviderServiceCollectionExtensions
    {
        public static IServiceCollection AddTargetProvider(this IServiceCollection @this)
        {
            @this.AddSingleton<ITargetProvider, ThreadSafeTargetProvider>(serviceProvider =>
            {
                IConfiguration configuration = serviceProvider.GetRequiredService<IConfiguration>();
                IOptions<CuberOptions> options = serviceProvider.GetRequiredService<IOptions<CuberOptions>>();
                return new ThreadSafeTargetProvider(options.Value.Targets); 
            });
            return @this;
        }
    }
}
