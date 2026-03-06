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

        public IReadOnlyList<CryptoPrice> GetPrices(string symbol)
        {
            var list = _exchanges
                .Select(e => new CryptoPrice(symbol, e.GetPrice(symbol), e.GetName()))
                .ToList();

            return list;
        }
    }
}