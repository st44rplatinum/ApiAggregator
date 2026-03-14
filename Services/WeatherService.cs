using System.Diagnostics;
using System.Text.Json;
using ApiAggregator.Models;
using Microsoft.Extensions.Caching.Memory;

namespace ApiAggregator.Services;

public class WeatherService : IAggregatorSource
{
    public string Key => "Weather";
    private const string Url = "https://api.openweathermap.org/data/2.5/weather?q=London&appid=YOUR_API_KEY&units=metric";
    private readonly IHttpClientFactory _http;
    private readonly IMemoryCache _cache;
    private readonly StatsService _stats;

    public WeatherService(IHttpClientFactory http, IMemoryCache cache, StatsService stats)
    {
        _http = http;
        _cache = cache;
        _stats = stats;
    }

    public async Task<object?> FetchAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(Url, out WeatherResponse? cached)) return cached;
        try
        {
            var client = _http.CreateClient();
            var sw = Stopwatch.StartNew();
            var response = await client.GetAsync(Url, cancellationToken);
            sw.Stop();
            _stats.Record("WeatherAPI", sw.ElapsedMilliseconds);
            if (!response.IsSuccessStatusCode) return null;
            var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var data = await JsonSerializer.DeserializeAsync<WeatherResponse>(stream, cancellationToken: cancellationToken);
            _cache.Set(Url, data, TimeSpan.FromMinutes(5));
            return data;
        }
        catch { return null; }
    }
}
