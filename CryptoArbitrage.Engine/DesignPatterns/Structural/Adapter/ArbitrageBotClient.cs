namespace CryptoArbitrage.Engine.DesignPatterns.Structural.Adapter;

/// <summary>
/// Client - Works only with the target interface.
/// </summary>
public class ArbitrageBotClient
{
    private readonly IExchangeMarketDataProvider _marketDataProvider;

    public ArbitrageBotClient(IExchangeMarketDataProvider marketDataProvider)
    {
        _marketDataProvider = marketDataProvider;
    }

    public string ScanPair(string tradingPair)
    {
        return _marketDataProvider.GetNormalizedTicker(tradingPair);
    }
}
