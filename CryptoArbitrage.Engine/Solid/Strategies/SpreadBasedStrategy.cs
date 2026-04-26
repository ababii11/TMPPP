using System.Collections.Generic;
using System.Linq;
using ArbitrageProject.Interfaces;
using ArbitrageProject.Models;

namespace ArbitrageProject.Strategies;

public class SpreadBasedStrategy : IArbitrageStrategy
{
    private readonly decimal _minSpreadAbsolute;
    private readonly decimal _minSpreadPercent;

    public string Name => "SpreadBasedStrategy";

    public SpreadBasedStrategy(decimal minSpreadAbsolute = 5m, decimal minSpreadPercent = 0.25m)
    {
        _minSpreadAbsolute = minSpreadAbsolute;
        _minSpreadPercent = minSpreadPercent;
    }

    public bool IsProfitable(decimal buyPrice, decimal sellPrice)
    {
        if (buyPrice <= 0m)
        {
            return false;
        }

        var spread = sellPrice - buyPrice;
        var spreadPercent = spread / buyPrice * 100m;
        return spread >= _minSpreadAbsolute && spreadPercent >= _minSpreadPercent;
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
                RouteDescription = "Need at least two exchanges for spread evaluation."
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
            RouteDescription = $"Spread route {buyLeg.Exchange}:{symbol} -> {sellLeg.Exchange}:{symbol}"
        };
    }
}