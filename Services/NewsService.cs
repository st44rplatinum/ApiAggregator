using System.Diagnostics;
using System.Text.Json;
using ApiAggregator.Models;
using Microsoft.Extensions.Caching.Memory;

namespace ApiAggregator.Services;

public class NewsService : IAggregatorSource
{
    public string Key => "News";
    private const string TopStoriesUrl = "https://hacker-news.firebaseio.com/v0/topstories.json";
    private readonly IHttpClientFactory _http;
    private readonly IMemoryCache _cache;
    private readonly StatsService _stats;

    public NewsService(IHttpClientFactory http, IMemoryCache cache, StatsService stats)
    {
        _http = http;
        _cache = cache;
        _stats = stats;
    }

    public async Task<object?> FetchAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(TopStoriesUrl, out HackerNewsStory? cached)) return cached;
        try
        {
            var client = _http.CreateClient();
            var sw = Stopwatch.StartNew();
            var idsResponse = await client.GetAsync(TopStoriesUrl, cancellationToken);
            sw.Stop();
            _stats.Record("HackerNews", sw.ElapsedMilliseconds);
            if (!idsResponse.IsSuccessStatusCode) return null;
            var idsStream = await idsResponse.Content.ReadAsStreamAsync(cancellationToken);
            var ids = await JsonSerializer.DeserializeAsync<List<int>>(idsStream, cancellationToken: cancellationToken);
            if (ids == null || ids.Count == 0) return null;
            var storyUrl = $"https://hacker-news.firebaseio.com/v0/item/{ids[0]}.json";
            var storyResponse = await client.GetAsync(storyUrl, cancellationToken);
            if (!storyResponse.IsSuccessStatusCode) return null;
            var storyStream = await storyResponse.Content.ReadAsStreamAsync(cancellationToken);
            var story = await JsonSerializer.DeserializeAsync<HackerNewsStory>(storyStream, cancellationToken: cancellationToken);
            _cache.Set(TopStoriesUrl, story, TimeSpan.FromMinutes(5));
            return story;
        }
        catch { return null; }
    }
}
