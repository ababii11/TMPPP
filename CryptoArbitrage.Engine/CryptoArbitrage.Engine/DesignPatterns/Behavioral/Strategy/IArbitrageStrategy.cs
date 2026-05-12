using System.Collections.Generic;
using ArbitrageProject.Models;

namespace CryptoArbitrage.Engine.DesignPatterns.Behavioral.Strategy;

/// <summary>
/// Strategy interface - declares common arbitrage evaluation behavior.
/// </summary>
public interface IArbitrageStrategy
{
    string StrategyName { get; }

    StrategyEvaluationResult Evaluate(
        string symbol,
        IReadOnlyList<CryptoPrice> prices,
        decimal feePercent);
}
