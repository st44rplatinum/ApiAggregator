using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;

public class AggregateTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AggregateTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private async Task<HttpClient> WithTokenAsync()
    {
        var client = _factory.CreateClient();
        var tokenResponse = await client.PostAsync("/api/token", null);
        tokenResponse.EnsureSuccessStatusCode();
        var json = await tokenResponse.Content.ReadAsStringAsync();
        var token = JsonSerializer.Deserialize<JsonElement>(json).GetProperty("token").GetString();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
        return client;
    }

    [Fact]
    public async Task Aggregate_ReturnsSuccess()
    {
        var client = await WithTokenAsync();
        var response = await client.GetAsync("/api/aggregate");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Stats_ReturnsSuccess()
    {
        var client = await WithTokenAsync();
        var response = await client.GetAsync("/api/stats");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Aggregate_CanDisableCrypto()
    {
        var client = await WithTokenAsync();
        var response = await client.GetAsync("/api/aggregate?includeCrypto=false");

        var body = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain("bitcoin", body);
    }
}