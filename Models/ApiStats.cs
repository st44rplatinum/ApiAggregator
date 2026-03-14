namespace ApiAggregator.Models;

public class ApiStats
{
    public int TotalRequests { get; set; }
    public long TotalResponseTime { get; set; }
    public int Fast { get; set; }
    public int Average { get; set; }
    public int Slow { get; set; }
    public double AvgResponseTime => TotalRequests == 0 ? 0 : (double)TotalResponseTime / TotalRequests;
}
