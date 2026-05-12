namespace CryptoArbitrage.Web.Services.MarketData;

/// <summary>
/// Store for real-time quotes from exchanges.
/// </summary>
public interface IQuoteStore
{
    /// <summary>
    /// Upsert a quote. Key format: "{exchange}:{symbol}" where exchange is lowercase, symbol is uppercase.
    /// </summary>
    void UpsertQuote(Quote quote);

    /// <summary>
    /// Get quote by exchange and symbol. Returns null if not found.
    /// </summary>
    Quote? GetQuote(string exchange, string symbol);

    /// <summary>
    /// Get all quotes for a symbol across exchanges.
    /// </summary>
    IEnumerable<Quote> GetQuotesBySymbol(string symbol);

    /// <summary>
    /// Clear all quotes (for testing/reset).
    /// </summary>
    void Clear();
}
