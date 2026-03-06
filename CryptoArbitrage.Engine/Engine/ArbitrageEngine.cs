using ArbitrageProject.Interfaces;

namespace ArbitrageProject.Engines;

public class ArbitrageEngine
{
    private readonly IArbitrageStrategy _strategy;
    private readonly IFeeCalculator _feeCalculator;

    // Engine now only depends on strategy and fee calculator (no provider).
    public ArbitrageEngine(IArbitrageStrategy strategy, IFeeCalculator feeCalculator)
    {
        _strategy = strategy;
        _feeCalculator = feeCalculator;
    }

    // Returns net profit per unit after fees; returns 0 or negative if not profitable.
    public decimal CheckOpportunity(decimal buyPrice, decimal sellPrice)
    {
        if (!_strategy.IsProfitable(buyPrice, sellPrice))
            return 0m;

        var gross = sellPrice - buyPrice;
        var feeOnBuy = _feeCalculator.CalculateFee(buyPrice);
        var feeOnSell = _feeCalculator.CalculateFee(sellPrice);
        var net = gross - feeOnBuy - feeOnSell;
        return net;
    }
}
