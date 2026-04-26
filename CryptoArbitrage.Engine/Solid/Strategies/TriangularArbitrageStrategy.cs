using System.Collections.Generic;
using System.Linq;
using ArbitrageProject.Interfaces;
using ArbitrageProject.Models;

namespace ArbitrageProject.Strategies;

public class TriangularArbitrageStrategy : IArbitrageStrategy
{
    private readonly decimal _minCycleReturnPercent;

    public string Name => "TriangularArbitrageStrategy";

    public TriangularArbitrageStrategy(decimal minCycleReturnPercent = 0.35m)
    {
        _minCycleReturnPercent = minCycleReturnPercent;
    }

    public bool IsProfitable(decimal buyPrice, decimal sellPrice)
    {
        if (buyPrice <= 0m)
        {
            return false;
        }

        var cycleReturnPercent = (sellPrice - buyPrice) / buyPrice * 100m;
        return cycleReturnPercent >= _minCycleReturnPercent;
    }

    public decimal CalculateProfit(decimal buyPrice, decimal sellPrice)
    {
        return sellPrice - buyPrice;
    }

    public ArbitrageOpportunity Evaluate(
        string symbol,
        IReadOnlyList<CryptoPrice> prices,
        IFeeCalculator feeCalculator)
    {
        if (prices.Count < 2)
        {
            return new ArbitrageOpportunity
            {
                StrategyName = Name,
                Symbol = symbol,
                RouteDescription = "Need at least two exchange prices to derive triangular cycle."
            };
        }

        var primaryLeg = prices.MinBy(p => p.Price)!;
        var exitLeg = prices.MaxBy(p => p.Price)!;

        var syntheticBridgePrice = (primaryLeg.Price + exitLeg.Price) / 2m;
        var cycleBuy = primaryLeg.Price;
        var cycleSell = (exitLeg.Price / syntheticBridgePrice) * primaryLeg.Price;

        var gross = CalculateProfit(cycleBuy, cycleSell);
        var feeTotal = feeCalculator.CalculateFee(cycleBuy)
                     + feeCalculator.CalculateFee(syntheticBridgePrice)
                     + feeCalculator.CalculateFee(cycleSell);
        var net = gross - feeTotal;

        return new ArbitrageOpportunity
        {
            StrategyName = Name,
            Symbol = symbol,
            BuyPrice = cycleBuy,
            SellPrice = cycleSell,
            ExchangeBuy = primaryLeg.Exchange,
            ExchangeSell = exitLeg.Exchange,
            GrossPerUnit = gross,
            NetPerUnit = net,
            IsProfitable = IsProfitable(cycleBuy, cycleSell) && net > 0m,
            RouteDescription = $"{symbol}/USDT -> USDT/ALT -> ALT/{symbol} using {primaryLeg.Exchange}/{exitLeg.Exchange}"
        };
    }
}