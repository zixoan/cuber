using System.Threading.Tasks;

using Zixoan.Cuber.Server.Config;

namespace Zixoan.Cuber.Server.Probe
{
    public interface IHealthProbe
    {
        Task<bool> IsReachable(Target target);
    }
}
