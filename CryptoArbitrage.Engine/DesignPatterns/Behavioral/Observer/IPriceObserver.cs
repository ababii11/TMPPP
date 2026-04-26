using System.Collections.Generic;
using ArbitrageProject.Models;

namespace CryptoArbitrage.Engine.DesignPatterns.Behavioral.Observer;

/// <summary>
/// Observer interface that receives price updates from the subject.
/// </summary>
public interface IPriceObserver
{
    string Name { get; }

    void Update(string symbol, IReadOnlyList<CryptoPrice> prices);
}
