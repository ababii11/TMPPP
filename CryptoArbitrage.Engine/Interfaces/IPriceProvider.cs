using System.Collections.Generic;
using ArbitrageProject.Models;

namespace ArbitrageProject.Interfaces
{
    public interface IPriceProvider
    {
        IReadOnlyList<CryptoPrice> GetPrices(string symbol);
    }
}
