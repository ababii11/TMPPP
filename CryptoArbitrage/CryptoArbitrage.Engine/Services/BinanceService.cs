using System;

namespace ArbitrageApp.Services
{
    public class BinanceService : ExchangeService
    {
        public BinanceService() : base("Binance") { }

        // Example stub: replace with real API call later
        public override decimal GetPrice(string symbol)
        {
            // deterministic stub for testing
            return symbol switch
            {
                "BTC" => 50000m,
                "ETH" => 3000m,
                _ => 1m
            };
        }
    }
}