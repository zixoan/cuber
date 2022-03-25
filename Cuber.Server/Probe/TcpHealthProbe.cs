using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using Zixoan.Cuber.Server.Config;

namespace Zixoan.Cuber.Server.Probe;

public class TcpHealthProbe : IHealthProbe
{
    private readonly CuberOptions cuberOptions;

    public TcpHealthProbe(IOptions<CuberOptions> options)
        => this.cuberOptions = options.Value;

    public async Task<bool> IsReachableAsync(Target target, CancellationToken cancellationToken = default)
    {
        try
        {
            using var timeoutCancellationTokenSource = new CancellationTokenSource(this.cuberOptions.HealthProbe!.Timeout);
            using var linkedCancellationTokenSource =
                CancellationTokenSource.CreateLinkedTokenSource(timeoutCancellationTokenSource.Token,
                    cancellationToken);
                
            using var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(target.Ip, this.cuberOptions.HealthProbe?.Port ?? target.Port,
                linkedCancellationTokenSource.Token);

            return true;
        }
        catch (Exception exception) when (
            exception is SocketException or ArgumentException or ObjectDisposedException or TaskCanceledException)
        {
            return false;
        }
    }
}