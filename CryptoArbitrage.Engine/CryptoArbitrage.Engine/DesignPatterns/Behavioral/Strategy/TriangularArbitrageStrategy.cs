using System.Collections.Generic;
using System.Linq;
using ArbitrageProject.Models;

namespace CryptoArbitrage.Engine.DesignPatterns.Behavioral.Strategy;

/// <summary>
/// ConcreteStrategy - simulates a triangular cycle (Asset -> Quote -> Bridge -> Asset).
/// </summary>
public class TriangularArbitrageStrategy : IArbitrageStrategy
{
    private readonly decimal _minCyclePercent;

    public string StrategyName => "TriangularArbitrageStrategy";

    public TriangularArbitrageStrategy(decimal minCyclePercent = 0.35m)
    {
        _minCyclePercent = minCyclePercent;
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
                Route = "Need at least two exchanges to derive cycle."
            };
        }

        var entryLeg = prices.MinBy(p => p.Price)!;
        var exitLeg = prices.MaxBy(p => p.Price)!;

        var syntheticBridgePrice = (entryLeg.Price + exitLeg.Price) / 2m;
        var cycleBuy = entryLeg.Price;
        var cycleSell = syntheticBridgePrice > 0m
            ? (exitLeg.Price / syntheticBridgePrice) * entryLeg.Price
            : 0m;

        var gross = cycleSell - cycleBuy;

        var feeLeg1 = cycleBuy * feePercent / 100m;
        var feeLeg2 = syntheticBridgePrice * feePercent / 100m;
        var feeLeg3 = cycleSell * feePercent / 100m;
        var net = gross - feeLeg1 - feeLeg2 - feeLeg3;

        var cyclePercent = cycleBuy > 0m ? (gross / cycleBuy) * 100m : 0m;
        var profitable = cyclePercent >= _minCyclePercent && net > 0m;

        return new StrategyEvaluationResult
        {
            StrategyName = StrategyName,
            Symbol = symbol,
            IsProfitable = profitable,
            BuyPrice = cycleBuy,
            SellPrice = cycleSell,
            BuyExchange = entryLeg.Exchange,
            SellExchange = exitLeg.Exchange,
            GrossProfitPerUnit = gross,
            NetProfitPerUnit = net,
            Route = $"{symbol}/USDT -> USDT/ALT -> ALT/{symbol} via {entryLeg.Exchange}/{exitLeg.Exchange}"
        };
    }
}
