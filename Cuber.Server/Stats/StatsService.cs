using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Zixoan.Cuber.Server.Stats;

public class StatsService : IStatsService
{
    private readonly IDictionary<string, ProxyStats> stats = new Dictionary<string, ProxyStats>();

    public void Add(string key, ProxyStats proxyStats)
        => this.stats.Add(key, proxyStats);

    public ProxyStats? Get(string key)
        => this.stats.TryGetValue(key, out ProxyStats? proxyStats) ? proxyStats : null;

    public IReadOnlyDictionary<string, ProxyStats> Get()
        => new ReadOnlyDictionary<string, ProxyStats>(stats);

    public void Remove(string key)
        => this.stats.Remove(key);
}
