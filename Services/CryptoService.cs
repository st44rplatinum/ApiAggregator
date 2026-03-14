using System.Diagnostics;
using System.Text.Json;
using ApiAggregator.Models;
using Microsoft.Extensions.Caching.Memory;

namespace ApiAggregator.Services;

public class CryptoService : IAggregatorSource
{
    public string Key => "Crypto";
    private const string Url = "https://api.coingecko.com/api/v3/simple/price?ids=bitcoin&vs_currencies=usd";
    private readonly IHttpClientFactory _http;
    private readonly IMemoryCache _cache;
    private readonly StatsService _stats;

    public CryptoService(IHttpClientFactory http, IMemoryCache cache, StatsService stats)
    {
        _http = http;
        _cache = cache;
        _stats = stats;
    }

    public async Task<object?> FetchAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(Url, out CryptoResponse? cached)) return cached;
        try
        {
            var client = _http.CreateClient();
            var sw = Stopwatch.StartNew();
            var response = await client.GetAsync(Url, cancellationToken);
            sw.Stop();
            _stats.Record("CryptoAPI", sw.ElapsedMilliseconds);
            if (!response.IsSuccessStatusCode) return null;
            var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var data = await JsonSerializer.DeserializeAsync<CryptoResponse>(stream, cancellationToken: cancellationToken);
            _cache.Set(Url, data, TimeSpan.FromMinutes(5));
            return data;
        }
        catch { return null; }
    }
}
