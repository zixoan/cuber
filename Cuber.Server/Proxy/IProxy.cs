namespace Zixoan.Cuber.Server.Proxy
{
    public interface IProxy
    {
        void Start(string ip, ushort port);
        void Stop();
    }
}
