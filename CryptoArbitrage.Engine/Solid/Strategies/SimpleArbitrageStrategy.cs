using System.Collections.Generic;
using System.Linq;
using ArbitrageProject.Interfaces;
using ArbitrageProject.Models;

namespace ArbitrageProject.Strategies;

public class SimpleArbitrageStrategy : IArbitrageStrategy
{
    private readonly decimal _minProfitPercent;
    public string Name => "SimpleArbitrageStrategy";

    // minProfitPercent is a percent (e.g. 0.5m == 0.5%)
    public SimpleArbitrageStrategy(decimal minProfitPercent = 0.5m) =>
        _minProfitPercent = minProfitPercent;

    public bool IsProfitable(decimal buyPrice, decimal sellPrice)
    {
        if (buyPrice <= 0) return false;
        var percent = (sellPrice - buyPrice) / buyPrice * 100m;
        return percent >= _minProfitPercent;
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
                RouteDescription = "Need at least two exchanges for cross-exchange arbitrage."
            };
        }

        var buyLeg = prices.MinBy(p => p.Price)!;
        var sellLeg = prices.MaxBy(p => p.Price)!;

        var gross = CalculateProfit(buyLeg.Price, sellLeg.Price);
        var net = gross - feeCalculator.CalculateFee(buyLeg.Price) - feeCalculator.CalculateFee(sellLeg.Price);

        return new ArbitrageOpportunity
        {
            StrategyName = Name,
            Symbol = symbol,
            BuyPrice = buyLeg.Price,
            SellPrice = sellLeg.Price,
            ExchangeBuy = buyLeg.Exchange,
            ExchangeSell = sellLeg.Exchange,
            GrossPerUnit = gross,
            NetPerUnit = net,
            IsProfitable = IsProfitable(buyLeg.Price, sellLeg.Price) && net > 0m,
            RouteDescription = $"{buyLeg.Exchange}:{symbol} -> {sellLeg.Exchange}:{symbol}"
        };
    }
}