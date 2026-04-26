using System.Collections.Generic;
using ArbitrageProject.Interfaces;
using ArbitrageProject.Models;
using CryptoArbitrage.Engine.DesignPatterns.Behavioral.Iterator;

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
            var collection = new ExchangeCollection(_exchanges);
            var iterator = collection.CreateIterator();
            var list = new List<CryptoPrice>();

            while (iterator.HasNext())
            {
                var exchange = iterator.Next();
                list.Add(new CryptoPrice(symbol, exchange.GetPrice(symbol), exchange.GetName()));
            }

            return list;
        }
    }
}