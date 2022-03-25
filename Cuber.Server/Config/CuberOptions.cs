using System.Collections.Generic;

namespace Zixoan.Cuber.Server.Config
{
    public class CuberOptions
    {
        private const int DefaultBufferSize = 8192;

        public string Ip { get; set; } = "127.0.0.1";
        public ushort Port { get; set; }
        public int Backlog { get; set; }
        public Mode Mode { get; set; }
        public BalanceStrategy BalanceStrategy { get; set; }
        public IEnumerable<Target>? Targets { get; set; }
        public HealthProbe? HealthProbe { get; set; }
        public int UpStreamBufferSize { get; set; } = DefaultBufferSize;
        public int DownStreamBufferSize { get; set; } = DefaultBufferSize;
        public Web Web { get; set; } = new();
    }

    public class Web
    {
        public string[] Urls { get; set; } = { "http://localhost:50001/" };
        public string ApiKeyHeaderName { get; set; } = "X-Api-Key";
        public string ApiKey { get; set; } = "changeme";
    }

    public class HealthProbe
    {
        public HealthProbeType? Type {get; set; }
        public ushort? Port { get; set; }
        public string Path { get; set; } = "/";
        public int Timeout { get; set; } = 5000;
        public int Interval { get; set; } = 5000;
    }

    public enum Mode
    {
        Tcp,
        Udp,
        Multi
    }

    public enum HealthProbeType
    {
        Tcp,
        Http
    }
}
