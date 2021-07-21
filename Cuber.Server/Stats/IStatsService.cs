using System.Collections.Generic;

#nullable enable
namespace Zixoan.Cuber.Server.Stats
{
    public interface IStatsService
    {
        void Add(string key, ProxyStats proxyStats);
        ProxyStats? Get(string key);
        IReadOnlyDictionary<string, ProxyStats> Get();
        void Remove(string key);
    }
}
#nullable disable
