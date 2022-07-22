using Zixoan.Cuber.Server.Config;

namespace Zixoan.Cuber.Server.Probe;

public interface IHealthProbe
{
    Task<bool> IsReachableAsync(Target target, CancellationToken cancellationToken = default);
}