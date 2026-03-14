using ApiAggregator.Models;
using ApiAggregator.Services;

namespace ApiAggregator.Endpoints;

public static class AggregateEndpoints
{
    public static void MapAggregateEndpoints(this WebApplication app)
    {
        app.MapGet("/api/aggregate", async (
            IEnumerable<IAggregatorSource> sources,
            string? include,
            bool? includeWeather,
            bool? includeNews,
            bool? includeCrypto,
            CancellationToken cancellationToken) =>
        {
            var keysToInclude = ResolveKeysToInclude(include, includeWeather, includeNews, includeCrypto);
            var sourcesToRun = sources.Where(s => keysToInclude.Contains(s.Key)).ToList();
            var tasks = sourcesToRun.Select(s => s.FetchAsync(cancellationToken)).ToList();
            var results = await Task.WhenAll(tasks);

            var byKey = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < sourcesToRun.Count; i++)
                byKey[sourcesToRun[i].Key] = results[i];

            var aggregated = new AggregatedResponse
            {
                Weather = byKey.GetValueOrDefault("Weather"),
                News = byKey.GetValueOrDefault("News"),
                Crypto = byKey.GetValueOrDefault("Crypto")
            };
            return Results.Ok(aggregated);
        }).RequireAuthorization();

        app.MapGet("/api/stats", (StatsService statsService) => Results.Ok(statsService.GetStats()))
            .RequireAuthorization();
    }

    private static HashSet<string> ResolveKeysToInclude(string? include, bool? includeWeather, bool? includeNews, bool? includeCrypto)
    {
        if (!string.IsNullOrWhiteSpace(include))
            return include.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => s.Trim())
                .Where(s => s.Length > 0)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (includeWeather != false) set.Add("Weather");
        if (includeNews != false) set.Add("News");
        if (includeCrypto != false) set.Add("Crypto");
        return set;
    }
}
