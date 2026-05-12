using System.Collections.Concurrent;

namespace CryptoArbitrage.Web.Services.MarketData;

/// <summary>
/// In-memory quote store backed by ConcurrentDictionary.
/// </summary>
public class QuoteStore : IQuoteStore
{
    private readonly ConcurrentDictionary<string, Quote> _quotes = new();

    public void UpsertQuote(Quote quote)
    {
        if (quote == null) return;
        var key = $"{quote.Exchange.ToLowerInvariant()}:{quote.Symbol.ToUpperInvariant()}";
        _quotes.AddOrUpdate(key, quote, (_, _) => quote);
    }

    public Quote? GetQuote(string exchange, string symbol)
    {
        var key = $"{exchange.ToLowerInvariant()}:{symbol.ToUpperInvariant()}";
        return _quotes.TryGetValue(key, out var quote) ? quote : null;
    }

    public IEnumerable<Quote> GetQuotesBySymbol(string symbol)
    {
        var upperSymbol = symbol.ToUpperInvariant();
        return _quotes.Values.Where(q => q.Symbol == upperSymbol);
    }

    public void Clear()
    {
        _quotes.Clear();
    }
}
