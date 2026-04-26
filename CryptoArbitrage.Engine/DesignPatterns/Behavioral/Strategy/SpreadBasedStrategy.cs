using System.Collections.Generic;
using System.Linq;
using ArbitrageProject.Models;

namespace CryptoArbitrage.Engine.DesignPatterns.Behavioral.Strategy;

/// <summary>
/// ConcreteStrategy - validates a trade only when absolute and percentage spread thresholds are met.
/// </summary>
public class SpreadBasedStrategy : IArbitrageStrategy
{
    private readonly decimal _minSpreadAbsolute;
    private readonly decimal _minSpreadPercent;

    public string StrategyName => "SpreadBasedStrategy";

    public SpreadBasedStrategy(decimal minSpreadAbsolute = 5m, decimal minSpreadPercent = 0.25m)
    {
        _minSpreadAbsolute = minSpreadAbsolute;
        _minSpreadPercent = minSpreadPercent;
    }

    public StrategyEvaluationResult Evaluate(
        string symbol,
        IReadOnlyList<CryptoPrice> prices,
        decimal feePercent)
    {
        if (prices.Count < 2)
        {
            return new StrategyEvaluationResult
            {
                StrategyName = StrategyName,
                Symbol = symbol,
                Route = "Need at least two exchanges."
            };
        }

        var buyLeg = prices.MinBy(p => p.Price)!;
        var sellLeg = prices.MaxBy(p => p.Price)!;

        var spread = sellLeg.Price - buyLeg.Price;
        var spreadPercent = buyLeg.Price > 0m ? spread / buyLeg.Price * 100m : 0m;

        var feeOnBuy = buyLeg.Price * feePercent / 100m;
        var feeOnSell = sellLeg.Price * feePercent / 100m;
        var net = spread - feeOnBuy - feeOnSell;

        var profitable = spread >= _minSpreadAbsolute
                         && spreadPercent >= _minSpreadPercent
                         && net > 0m;

        return new StrategyEvaluationResult
        {
            StrategyName = StrategyName,
            Symbol = symbol,
            IsProfitable = profitable,
            BuyPrice = buyLeg.Price,
            SellPrice = sellLeg.Price,
            BuyExchange = buyLeg.Exchange,
            SellExchange = sellLeg.Exchange,
            GrossProfitPerUnit = spread,
            NetProfitPerUnit = net,
            Route = $"Spread filter: {buyLeg.Exchange}:{symbol} -> {sellLeg.Exchange}:{symbol}"
        };
    }
}
