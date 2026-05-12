namespace CryptoArbitrage.Web.Services.MarketData;

/// <summary>
/// Represents a real-time bid/ask quote from an exchange.
/// </summary>
public record Quote(
    string Exchange,
    string Symbol,
    decimal Bid,
    decimal Ask,
    DateTime TimestampUtc,
    string SourceSymbol);
