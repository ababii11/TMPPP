namespace CryptoArbitrage.Engine.DesignPatterns.Structural.Adapter;

/// <summary>
/// Target - Interface expected by the client.
/// </summary>
public interface IExchangeMarketDataProvider
{
    /// <summary>
    /// Returns normalized market data for a trading pair (ex: BTC/USDT).
    /// </summary>
    string GetNormalizedTicker(string tradingPair);
}
