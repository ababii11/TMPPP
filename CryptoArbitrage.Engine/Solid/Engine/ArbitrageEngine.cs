using ArbitrageProject.Interfaces;
using ArbitrageProject.Models;

namespace ArbitrageProject.Engines;

public class ArbitrageEngine
{
    private IArbitrageStrategy _strategy;
    private readonly IFeeCalculator _feeCalculator;

    // Engine now only depends on strategy and fee calculator (no provider).
    public ArbitrageEngine(IArbitrageStrategy strategy, IFeeCalculator feeCalculator)
    {
        _strategy = strategy;
        _feeCalculator = feeCalculator;
    }

    public void SetStrategy(IArbitrageStrategy strategy)
    {
        _strategy = strategy;
    }

    public string GetCurrentStrategyName() => _strategy.Name;

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

    public ArbitrageOpportunity EvaluateOpportunity(string symbol, IReadOnlyList<CryptoPrice> prices)
    {
        return _strategy.Evaluate(symbol, prices, _feeCalculator);
    }
}
