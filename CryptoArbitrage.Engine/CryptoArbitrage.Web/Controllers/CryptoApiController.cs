using Microsoft.AspNetCore.Mvc;
using ArbitrageProject.Engines;
using ArbitrageProject.Interfaces;
using ArbitrageProject.Services;
using ArbitrageProject.Strategies;
using CryptoArbitrage.Engine.DesignPatterns.Behavioral.Observer;
using CryptoArbitrage.Engine.DesignPatterns.Behavioral.Memento;

namespace CryptoArbitrage.Web.Controllers;

[ApiController]
[Route("api/crypto")]
public class CryptoApiController : ControllerBase
{
    private readonly CryptoPriceService _priceSubject;
    private readonly ArbitrageBotStateOriginator _botState;

    public CryptoApiController(CryptoPriceService priceSubject, ArbitrageBotStateOriginator botState)
    {
        _priceSubject = priceSubject;
        _botState = botState;
    }

    [HttpGet("prices")]
    public IActionResult GetPrices()
    {
        var symbols = new[] { "BTC", "ETH", "SOL", "ADA", "XRP" };
        var prices = symbols
            .SelectMany(symbol => _priceSubject.SimulatePriceTick(symbol))
            .GroupBy(p => p.Symbol)
            .Select(g => new
            {
                symbol = g.Key,
                price = g.Average(x => x.Price)
            })
            .ToArray();

        return Ok(prices);
    }

    [HttpGet("prices-live")]
    public IActionResult GetLivePrices(
        [FromQuery] string symbol = "BTC",
        [FromQuery] string strategy = "simple",
        [FromQuery] decimal feePercent = 0.1m,
        [FromQuery] decimal minProfitPercent = 0.5m,
        [FromQuery] decimal spreadAlertPercent = 0.2m)
    {
        symbol = (symbol ?? "BTC").Trim().ToUpperInvariant();

        var observerEvents = new List<string>();
        void Capture(string message) => observerEvents.Add(message);

        var engine = new ArbitrageEngine(
            BuildStrategy(strategy, minProfitPercent),
            new FeeCalculator(feePercent));

        _botState.SetActiveStrategy(engine.GetCurrentStrategyName());

        var logger = new LoggerService(Capture);
        var alert = new AlertService(spreadAlertPercent, Capture);
        var bot = new ArbitrageBot(engine, Capture);

        _priceSubject.Attach(logger);
        _priceSubject.Attach(alert);
        _priceSubject.Attach(bot);

        try
        {
            var tickPrices = _priceSubject.SimulatePriceTick(symbol);
            var opportunity = engine.EvaluateOpportunity(symbol, tickPrices);

            return Ok(new
            {
                symbol,
                strategy = engine.GetCurrentStrategyName(),
                prices = tickPrices.Select(p => new { symbol = p.Symbol, price = p.Price, exchange = p.Exchange }),
                opportunity = new
                {
                    buyPrice = opportunity.BuyPrice,
                    sellPrice = opportunity.SellPrice,
                    exchangeBuy = opportunity.ExchangeBuy,
                    exchangeSell = opportunity.ExchangeSell,
                    netPerUnit = opportunity.NetPerUnit,
                    isProfitable = opportunity.IsProfitable,
                    route = opportunity.RouteDescription
                },
                observerEvents
            });
        }
        finally
        {
            _priceSubject.Detach(logger);
            _priceSubject.Detach(alert);
            _priceSubject.Detach(bot);
        }
    }

    private static IArbitrageStrategy BuildStrategy(string strategy, decimal minProfitPercent)
    {
        return strategy.ToLowerInvariant() switch
        {
            "triangular" => new TriangularArbitrageStrategy(minProfitPercent),
            "spread" => new SpreadBasedStrategy(5m, minProfitPercent),
            _ => new SimpleArbitrageStrategy(minProfitPercent)
        };
    }
}
