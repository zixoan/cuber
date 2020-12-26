using System;

using Microsoft.Extensions.Options;

using Zixoan.Cuber.Server.Config;

namespace Zixoan.Cuber.Server.Probe
{
    public class HealthProbeFactory
    {
        public static IHealthProbe Create(HealthProbeType type, CuberOptions cuberOptions)
        {
            IOptions<CuberOptions> options = Options.Create(cuberOptions);

            switch (type)
            {
                case HealthProbeType.Tcp:
                    return new TcpHealthProbe(options);
                case HealthProbeType.Http:
                    return new HttpHealthProbe(options);
                default:
                    throw new ArgumentException($"Unknown health probe type, only tcp is currently supported");
            }
        }
    }
}
