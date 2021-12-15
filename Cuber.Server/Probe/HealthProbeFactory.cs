using System;

using Microsoft.Extensions.Options;

using Zixoan.Cuber.Server.Config;

namespace Zixoan.Cuber.Server.Probe
{
    public static class HealthProbeFactory
    {
        public static IHealthProbe Create(HealthProbeType type, IOptions<CuberOptions> cuberOptions)
        {
            return type switch
            {
                HealthProbeType.Tcp => new TcpHealthProbe(cuberOptions),
                HealthProbeType.Http => new HttpHealthProbe(cuberOptions),
                _ => throw new ArgumentException(
                    "Unknown health probe type, only tcp and http are currently supported")
            };
        }
    }
}
