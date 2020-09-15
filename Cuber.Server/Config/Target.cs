using System;

namespace Zixoan.Cuber.Server.Config
{
    public class Target
    {
        public string Ip { get; set; }
        public int Port { get; set; }

        public override bool Equals(object obj)
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
