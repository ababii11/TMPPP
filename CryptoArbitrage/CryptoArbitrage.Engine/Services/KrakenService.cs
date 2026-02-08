using System;

namespace ArbitrageApp.Services
{
    public class KrakenService : ExchangeService
    {
        public KrakenService() : base("Kraken") { }

        // Example stub: replace with real API call later
        public override decimal GetPrice(string symbol)
        {
            // deterministic stub for testing
            return symbol switch
            {
                "BTC" => 50100m,
                "ETH" => 2980m,
                _ => 1m
            };
        }
    }
}