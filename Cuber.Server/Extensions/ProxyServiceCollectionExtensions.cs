using System.Collections.Generic;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Zixoan.Cuber.Server.Balancing;
using Zixoan.Cuber.Server.Config;
using Zixoan.Cuber.Server.Proxy;
using Zixoan.Cuber.Server.Proxy.Multi;
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

                switch (options.Value.Mode)
                {
                    case Mode.Udp:
                        return new UdpProxy(loggerFactory.CreateLogger<UdpProxy>(), options, loadBalanceStrategy);
                    case Mode.Multi:
                    {
                        TcpProxy tcpProxy = new TcpProxy(loggerFactory.CreateLogger<TcpProxy>(), options, loadBalanceStrategy);
                        UdpProxy udpProxy = new UdpProxy(loggerFactory.CreateLogger<UdpProxy>(), options, loadBalanceStrategy);
                        return new MultiProxy(loggerFactory.CreateLogger<MultiProxy>(), new List<IProxy> { tcpProxy, udpProxy }, loadBalanceStrategy);
                    }
                    default:
                        return new TcpProxy(loggerFactory.CreateLogger<TcpProxy>(), options, loadBalanceStrategy);
                }
            });
            return @this;
        }
    }
}
