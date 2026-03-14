using System.Collections.Concurrent;
using ApiAggregator.Models;

namespace ApiAggregator.Services;

public class StatsService
{
    private readonly ConcurrentDictionary<string, ApiStats> _stats = new();

    public void Record(string apiName, long responseTime)
    {
        var stat = _stats.GetOrAdd(apiName, _ => new ApiStats());
        stat.TotalRequests++;
        stat.TotalResponseTime += responseTime;
        if (responseTime < 100) stat.Fast++;
        else if (responseTime < 200) stat.Average++;
        else stat.Slow++;
    }

    public object GetStats() => _stats;
}
