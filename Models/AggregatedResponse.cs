namespace ApiAggregator.Models;

public class AggregatedResponse
{
    public object? Weather { get; set; }
    public object? News { get; set; }
    public object? Crypto { get; set; }
}
