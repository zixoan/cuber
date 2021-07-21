using Microsoft.Extensions.DependencyInjection;

using Zixoan.Cuber.Server.Stats;

namespace Zixoan.Cuber.Server.Extensions
{
    public static class StatsServiceCollectionExtensions
    {
        public static IServiceCollection AddStats(this IServiceCollection @this)
        {
            @this.AddSingleton<IStatsService, StatsService>();
            return @this;
        }
    }
}
