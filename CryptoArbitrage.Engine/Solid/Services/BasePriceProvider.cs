using System.Collections.Generic;
using ArbitrageProject.Interfaces;
using ArbitrageProject.Models;

namespace ArbitrageProject.Services;

public abstract class BasePriceProvider : IPriceProvider
{
    public abstract IReadOnlyList<CryptoPrice> GetPrices(string symbol);
}