using System.Collections.Generic;
using ArbitrageProject.Models;

namespace ArbitrageProject.Interfaces;

public interface IArbitrageStrategy
{
    string Name { get; }

    bool IsProfitable(decimal buyPrice, decimal sellPrice);

    decimal CalculateProfit(decimal buyPrice, decimal sellPrice);

    ArbitrageOpportunity Evaluate(
        string symbol,
        IReadOnlyList<CryptoPrice> prices,
        IFeeCalculator feeCalculator);
}