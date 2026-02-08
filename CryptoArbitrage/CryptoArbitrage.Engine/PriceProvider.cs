using System.Collections.Generic;
using System.Linq;
using ArbitrageProject.Interfaces;
using ArbitrageProject.Models;

namespace ArbitrageProject.Services
{
    public class PriceProvider : IPriceProvider
    {
        private readonly IEnumerable<IExchangeService> _exchanges;

        public PriceProvider(IEnumerable<IExchangeService> exchanges)
        {
            _exchanges = exchanges;
        }

        // Returns a list of prices (one per configured exchange) for the given symbol.
        public IReadOnlyList<CryptoPrice> GetPrices(string symbol)
        {
            var list = _exchanges.Select(e => new CryptoPrice(symbol, e.GetName(), e.GetPrice(symbol))).ToList();
            return list;
        }
    }
}
