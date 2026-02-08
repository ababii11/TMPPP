using System.Collections.Generic;
using System.Linq;
using ArbitrageApp.Engine;
using ArbitrageApp.Models;
using ArbitrageApp.Providers;

namespace ArbitrageApp.ViewModels
{
    public class MainViewModel
    {
        private readonly ArbitrageEngine _engine;
        private readonly IPriceProvider _provider;

        public IReadOnlyList<string> OpportunityDescriptions { get; private set; } = new List<string>();

        public MainViewModel(ArbitrageEngine engine, IPriceProvider provider)
        {
            _engine = engine;
            _provider = provider;
        }

        // Refresh fetches prices and finds cross-exchange opportunities.
        public void Refresh()
        {
            _provider.FetchPrices();

            var prices = _provider.Prices;
            var opportunities = new List<string>();

            // Check every pair of prices on different exchanges for same symbol
            var pairs = from buy in prices
                        from sell in prices
                        where buy.Symbol == sell.Symbol && buy.Exchange != sell.Exchange
                        select (buy, sell);

            foreach (var (buy, sell) in pairs)
            {
                var netPerUnit = _engine.CheckOpportunity(buy.Price, sell.Price);
                if (netPerUnit > 0)
                {
                    opportunities.Add($"{buy.Symbol}: Buy @ {buy.Price} ({buy.Exchange}) -> Sell @ {sell.Price} ({sell.Exchange}) => Net/unit {netPerUnit:F2}");
                }
            }

            OpportunityDescriptions = opportunities;
        }
    }
}