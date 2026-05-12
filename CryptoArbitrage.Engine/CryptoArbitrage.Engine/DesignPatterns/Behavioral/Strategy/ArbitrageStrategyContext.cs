using System.Collections.Generic;
using ArbitrageProject.Models;

namespace CryptoArbitrage.Engine.DesignPatterns.Behavioral.Strategy;

/// <summary>
/// Context - keeps a reference to a strategy and delegates arbitrage evaluation.
/// </summary>
public class ArbitrageStrategyContext
{
    private IArbitrageStrategy _strategy;

    public ArbitrageStrategyContext(IArbitrageStrategy strategy)
    {
        _strategy = strategy;
    }

    public void SetStrategy(IArbitrageStrategy strategy)
    {
        _strategy = strategy;
    }

    public StrategyEvaluationResult Analyze(
        string symbol,
        IReadOnlyList<CryptoPrice> prices,
        decimal feePercent)
    {
        return _strategy.Evaluate(symbol, prices, feePercent);
    }
}
