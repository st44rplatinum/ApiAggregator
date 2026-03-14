using System.Text.Json.Serialization;

namespace ApiAggregator.Models;

public class CryptoResponse
{
    [JsonPropertyName("bitcoin")]
    public Bitcoin? Bitcoin { get; set; }
}

public class Bitcoin
{
    [JsonPropertyName("usd")]
    public decimal Usd { get; set; }
}
