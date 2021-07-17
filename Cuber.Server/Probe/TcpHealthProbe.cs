using System;
using System.Net.Sockets;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using Zixoan.Cuber.Server.Config;

namespace Zixoan.Cuber.Server.Probe
{
    public class TcpHealthProbe : IHealthProbe
    {
        private readonly CuberOptions cuberOptions;

        public TcpHealthProbe(IOptions<CuberOptions> options)
        {
            this.cuberOptions = options.Value;
        }

        public async Task<bool> IsReachable(Target target)
        {
            try
            {
                using TcpClient tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(target.Ip, this.cuberOptions.HealthProbe?.Port ?? target.Port);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
