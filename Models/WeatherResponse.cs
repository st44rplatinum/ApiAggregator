namespace ApiAggregator.Models;

public class WeatherResponse
{
    public string? Name { get; set; }
    public MainInfo? Main { get; set; }
}

public class MainInfo
{
    public double Temp { get; set; }
}
