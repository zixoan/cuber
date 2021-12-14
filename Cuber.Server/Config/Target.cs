using System;
using System.Threading;

namespace Zixoan.Cuber.Server.Config
{
    public class Target
    {
        private int connections;

        public string Ip { get; set; }
        public ushort Port { get; set; }
        public int Connections => this.connections;

        public Target(string ip, ushort port)
        {
            this.Ip = ip;
            this.Port = port;
        }

        public void IncrementConnections()
            => Interlocked.Increment(ref this.connections);

        public void DecrementConnections()
            => Interlocked.Decrement(ref this.connections);

        public override bool Equals(object? obj)
        {
            if (!(obj is Target target))
            {
                return false;
            }

            return Ip == target.Ip &&
                   Port == target.Port;
        }

        public override int GetHashCode()
            => HashCode.Combine(Ip, Port);

        public override string ToString()
            => $"{this.Ip}:{this.Port}";
    }
}
