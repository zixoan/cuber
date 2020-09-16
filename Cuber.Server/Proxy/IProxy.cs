namespace Zixoan.Cuber.Server.Proxy
{
    public interface IProxy
    {
        public void Listen(string ip, ushort port);
        public void Stop();
    }
}
