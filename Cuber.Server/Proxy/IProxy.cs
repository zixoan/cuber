namespace Zixoan.Cuber.Server.Proxy
{
    public interface IProxy
    {
        void Listen(string ip, ushort port);
        void Stop();
    }
}
