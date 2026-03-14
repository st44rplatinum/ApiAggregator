namespace ApiAggregator.Services;

/// <summary>
/// A single external API that contributes to the aggregate.
/// To add a new API: implement this, register in DI, then use include=YourKey in the aggregate request.
/// </summary>
public interface IAggregatorSource
{
    string Key { get; }
    Task<object?> FetchAsync(CancellationToken cancellationToken = default);
}
