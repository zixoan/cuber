namespace Zixoan.Cuber.Server.Config;

public class Target
{
    private int connections;

    public string Ip { get; } = null!;
    public ushort Port { get; }
    public int Connections => this.connections;

    // ReSharper disable once UnusedMember.Global
    public Target()
    {
            
    }
        
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
        if (obj is not Target target)
        {
            return false;
        }
        
        return this.Ip == target.Ip &&
               this.Port == target.Port;
    }
        
    public override int GetHashCode()
        => HashCode.Combine(this.Ip, this.Port);
        
    public override string ToString()
        => $"{this.Ip}:{this.Port}";
}
