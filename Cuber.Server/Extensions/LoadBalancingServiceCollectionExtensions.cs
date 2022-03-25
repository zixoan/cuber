using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Zixoan.Cuber.Server.Balancing;
using Zixoan.Cuber.Server.Config;
using Zixoan.Cuber.Server.Provider;

namespace Zixoan.Cuber.Server.Extensions;

public static class LoadBalancingServiceCollectionExtensions
{
    public static IServiceCollection AddLoadBalancing(this IServiceCollection @this)
    {
        @this.AddSingleton(serviceProvider =>
        {
            IOptions<CuberOptions> options = serviceProvider.GetRequiredService<IOptions<CuberOptions>>();
            ITargetProvider targetProvider = serviceProvider.GetRequiredService<ITargetProvider>();
            return LoadBalanceStrategyFactory.Create(options.Value.BalanceStrategy, targetProvider);
        });
        return @this;
    }
}