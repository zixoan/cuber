using System;
using System.Net.Sockets;
using System.Threading.Tasks;

using Zixoan.Cuber.Server.Config;

namespace Zixoan.Cuber.Server.Probe
{
    public class TcpHealthProbe : IHealthProbe
    {
        public async Task<bool> IsReachable(Target target)
        {
            try
            {
                using TcpClient tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(target.Ip, target.Port);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
