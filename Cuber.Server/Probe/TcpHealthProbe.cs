using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using Zixoan.Cuber.Server.Config;

namespace Zixoan.Cuber.Server.Probe
{
    public class TcpHealthProbe : IHealthProbe
    {
        private readonly CuberOptions cuberOptions;

        public TcpHealthProbe(IOptions<CuberOptions> options)
            => this.cuberOptions = options.Value;

        public async Task<bool> IsReachable(Target target)
        {
            try
            {
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(this.cuberOptions.HealthProbe.Timeout);

                using TcpClient tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(target.Ip, this.cuberOptions.HealthProbe?.Port ?? target.Port, cancellationTokenSource.Token);

                return true;
            }
            catch (Exception exception) when (
                exception is SocketException ||
                exception is ArgumentException ||
                exception is ObjectDisposedException ||
                exception is TaskCanceledException)
            {
                return false;
            }
        }
    }
}
