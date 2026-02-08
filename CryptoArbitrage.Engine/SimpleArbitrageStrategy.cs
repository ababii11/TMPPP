using ArbitrageProject.Interfaces;

namespace ArbitrageProject.Strategies;

public class SimpleArbitrageStrategy : IArbitrageStrategy
{
    private readonly decimal _minProfitPercent;

    // minProfitPercent is a percent (e.g. 0.5m == 0.5%)
    public SimpleArbitrageStrategy(decimal minProfitPercent = 0.5m) => _minProfitPercent = minProfitPercent;

    public bool IsProfitable(decimal buyPrice, decimal sellPrice)
    {
        if (buyPrice <= 0) return false;
        var percent = (sellPrice - buyPrice) / buyPrice * 100m;
        return percent >= _minProfitPercent;
    }
}
