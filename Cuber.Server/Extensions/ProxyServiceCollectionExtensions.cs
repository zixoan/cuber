using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Zixoan.Cuber.Server.Balancing;
using Zixoan.Cuber.Server.Config;
using Zixoan.Cuber.Server.Proxy;
using Zixoan.Cuber.Server.Proxy.Tcp;
using Zixoan.Cuber.Server.Proxy.Udp;

namespace Zixoan.Cuber.Server.Extensions
{
    public static class ProxyServiceCollectionExtensions
    {
        public static IServiceCollection AddProxy(this IServiceCollection @this)
        {
            @this.AddSingleton<IProxy, ProxyBase>(serviceProvider =>
            {
                IOptions<CuberOptions> options = serviceProvider.GetRequiredService<IOptions<CuberOptions>>();
                
                ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                ILoadBalanceStrategy loadBalanceStrategy = serviceProvider.GetRequiredService<ILoadBalanceStrategy>();

                ProxyBase proxy = options.Value.Mode switch
                {
                    Mode.Udp => new UdpProxy(loggerFactory.CreateLogger<UdpProxy>(), options, loadBalanceStrategy),
                    _ => new TcpProxy(loggerFactory.CreateLogger<TcpProxy>(), options, loadBalanceStrategy),
                };
                return proxy;
            });
            return @this;
        }
    }
}
