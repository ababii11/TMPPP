namespace CryptoArbitrage.Engine.DesignPatterns.Structural.Adapter;

/// <summary>
/// Adapter - Converts client calls to the legacy service format.
/// </summary>
public class LegacyExchangeTickerAdapter : IExchangeMarketDataProvider
{
    private readonly LegacyExchangeTickerService _adaptee;

    public LegacyExchangeTickerAdapter(LegacyExchangeTickerService adaptee)
    {
        _adaptee = adaptee;
    }

    public string GetNormalizedTicker(string tradingPair)
    {
        var parts = tradingPair.Split('/');
        var baseAsset = parts.Length > 0 ? parts[0] : tradingPair;
        var quoteAsset = parts.Length > 1 ? parts[1] : "USDT";

        return _adaptee.GetTickerLegacyFormat(baseAsset, quoteAsset);
    }
}
