using System.Collections.Generic;

namespace Zixoan.Cuber.Server.Config
{
    public class CuberOptions
    {
        public string Ip { get; set; }
        public ushort Port { get; set; }
        public Mode Mode { get; set; }
        public BalanceStrategy BalanceStrategy { get; set; }
        public List<Target> Targets { get; set; }
    }
}
