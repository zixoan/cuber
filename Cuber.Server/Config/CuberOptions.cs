using System.Collections.Generic;

#nullable enable
namespace Zixoan.Cuber.Server.Config
{
    public class CuberOptions
    {
        private const int DefaultBufferSize = 8192;

        public string? Ip { get; set; }
        public ushort Port { get; set; }
        public Mode Mode { get; set; }
        public BalanceStrategy BalanceStrategy { get; set; }
        public List<Target>? Targets { get; set; }
        public HealthProbe? HealthProbe { get; set; }
        public int UpStreamBufferSize { get; set; } = DefaultBufferSize;
        public int DownStreamBufferSize { get; set; } = DefaultBufferSize;
    }

    public class HealthProbe
    {
        public HealthProbeType? Type {get; set; }
        public ushort? Port { get; set; }
        public string? Path { get; set; }
        public int? Timeout { get; set; }
    }

    public enum Mode
    {
        Tcp,
        Udp
    }

    public enum HealthProbeType
    {
        Tcp,
        Http
    }
}
#nullable disable
