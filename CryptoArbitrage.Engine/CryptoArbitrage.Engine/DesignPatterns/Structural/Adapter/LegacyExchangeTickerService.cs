namespace CryptoArbitrage.Engine.DesignPatterns.Structural.Adapter;

/// <summary>
/// Adaptee/Service - Existing legacy API with an incompatible signature.
/// </summary>
public class LegacyExchangeTickerService
{
    /// <summary>
    /// Legacy method expected by older integrations.
    /// </summary>
    public string GetTickerLegacyFormat(string baseAsset, string quoteAsset)
    {
        return $"Legacy ticker payload for {baseAsset}-{quoteAsset}";
    }
}
