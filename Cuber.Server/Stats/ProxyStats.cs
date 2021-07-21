using System;
using System.Threading;

namespace Zixoan.Cuber.Server.Stats
{
    public class ProxyStats
    {
        private long startUnixSeconds;

        public ProxyStats(long startUnixSeconds)
            => this.startUnixSeconds = startUnixSeconds;

        private int activeConnections;
        private ulong upstreamReceived;
        private ulong upstreamSent;
        private ulong downstreamReceived;
        private ulong downstreamSent;

        public long Uptime => DateTimeOffset.Now.ToUnixTimeSeconds() - this.startUnixSeconds;
        public int ActiveConnections => this.activeConnections;
        public ulong UpstreamReceived => this.upstreamReceived;
        public ulong UpstreamSent => this.upstreamSent;
        public ulong UpstreamSentPackets => this.upstreamSent;
        public ulong DownstreamReceived => this.downstreamReceived;
        public ulong DownstreamSent => this.downstreamSent;

        public void IncrementActiveConnections()
            => Interlocked.Increment(ref this.activeConnections);

        public void DecrementActiveConnections()
            => Interlocked.Decrement(ref this.activeConnections);

        public void IncrementUpstreamReceived(int amount)
            => Interlocked.Add(ref this.upstreamReceived, (uint)amount);

        public void IncrementUpstreamSent(int amount)
            => Interlocked.Add(ref this.upstreamSent, (uint)amount);

        public void IncrementDownstreamReceived(int amount)
            => Interlocked.Add(ref this.downstreamReceived, (uint)amount);

        public void IncrementDownstreamSent(int amount)
            => Interlocked.Add(ref this.downstreamSent, (uint)amount);
    }
}
