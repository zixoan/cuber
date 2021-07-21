using System.Collections.Generic;

using Zixoan.Cuber.Server.Stats;

public class NullStatsService : IStatsService
{
    public void Add(string key, ProxyStats stats)
    {

    }

    public ProxyStats Get(string key)
    {
        return null;
    }

    public IReadOnlyDictionary<string, ProxyStats> Get()
    {
        return new Dictionary<string, ProxyStats>();
    }

    public void Remove(string key)
    {
    }
}
