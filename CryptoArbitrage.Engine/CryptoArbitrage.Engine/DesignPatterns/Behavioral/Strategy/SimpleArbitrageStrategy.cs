using System.Collections.Generic;
using System.Linq;
using ArbitrageProject.Models;

namespace CryptoArbitrage.Engine.DesignPatterns.Behavioral.Strategy;

/// <summary>
/// ConcreteStrategy - direct buy on cheapest exchange and sell on most expensive exchange.
/// </summary>
public class SimpleArbitrageStrategy : IArbitrageStrategy
{
    private readonly decimal _minProfitPercent;

    public string StrategyName => "SimpleArbitrageStrategy";

    public SimpleArbitrageStrategy(decimal minProfitPercent = 0.5m)
    {
        _minProfitPercent = minProfitPercent;
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

        var gross = sellLeg.Price - buyLeg.Price;
        var feeOnBuy = buyLeg.Price * feePercent / 100m;
        var feeOnSell = sellLeg.Price * feePercent / 100m;
        var net = gross - feeOnBuy - feeOnSell;

        var percent = buyLeg.Price > 0m ? (gross / buyLeg.Price) * 100m : 0m;
        var profitable = percent >= _minProfitPercent && net > 0m;

        return new StrategyEvaluationResult
        {
            StrategyName = StrategyName,
            Symbol = symbol,
            IsProfitable = profitable,
            BuyPrice = buyLeg.Price,
            SellPrice = sellLeg.Price,
            BuyExchange = buyLeg.Exchange,
            SellExchange = sellLeg.Exchange,
            GrossProfitPerUnit = gross,
            NetProfitPerUnit = net,
            Route = $"{buyLeg.Exchange}:{symbol} -> {sellLeg.Exchange}:{symbol}"
        };
    }
}
