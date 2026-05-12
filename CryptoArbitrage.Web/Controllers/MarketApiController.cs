using Microsoft.AspNetCore.Mvc;
using CryptoArbitrage.Web.Services.MarketData;

namespace CryptoArbitrage.Web.Controllers;

[ApiController]
[Route("api/market")]
public class MarketApiController : ControllerBase
{
    private readonly IQuoteStore _quoteStore;

    public MarketApiController(IQuoteStore quoteStore)
    {
        _quoteStore = quoteStore;
    }

    [HttpGet("spread")]
    public IActionResult GetSpread([FromQuery] string? symbol)
    {
        // Validate symbol
        var normalizedSymbol = (symbol ?? "").ToUpperInvariant();
        if (!new[] { "BTC", "ETH" }.Contains(normalizedSymbol))
        {
            return BadRequest(new { message = "Symbol must be BTC or ETH" });
        }

        // Get quotes from both exchanges
        var binanceQuote = _quoteStore.GetQuote("binance", normalizedSymbol);
        var krakenQuote = _quoteStore.GetQuote("kraken", normalizedSymbol);

        // Both required
        if (binanceQuote == null || krakenQuote == null)
        {
            return NotFound(new { message = "Quotes not ready yet, try again" });
        }

        // Use the latest timestamp
        var ts = new[] { binanceQuote.TimestampUtc, krakenQuote.TimestampUtc }.Max();

        // Find best buy (lowest ask) and best sell (highest bid)
        var quotes = new[] { binanceQuote, krakenQuote };
        var buyQuote = quotes.MinBy(q => q.Ask)!;
        var sellQuote = quotes.MaxBy(q => q.Bid)!;

        var grossSpread = sellQuote.Bid - buyQuote.Ask;
        var grossSpreadPct = buyQuote.Ask > 0 
            ? (grossSpread / buyQuote.Ask) * 100m 
            : 0m;

        return Ok(new
        {
            symbol = normalizedSymbol,
            timestampUtc = ts,
            exchanges = new[]
            {
                new
                {
                    name = "binance",
                    bid = binanceQuote.Bid,
                    ask = binanceQuote.Ask,
                    sourceSymbol = binanceQuote.SourceSymbol
                },
                new
                {
                    name = "kraken",
                    bid = krakenQuote.Bid,
                    ask = krakenQuote.Ask,
                    sourceSymbol = krakenQuote.SourceSymbol
                }
            },
            best = new
            {
                buyOn = buyQuote.Exchange,
                sellOn = sellQuote.Exchange,
                grossSpread = Math.Round(grossSpread, 8),
                grossSpreadPct = Math.Round(grossSpreadPct, 4)
            }
        });
    }
}
