using ArbitrageProject.Interfaces;
using ArbitrageProject.Models;

namespace ArbitrageProject.Services;

public abstract class BasePriceProvider : IPriceProvider
{
    public abstract List<CryptoPrice> GetPrices(string symbol);
}
