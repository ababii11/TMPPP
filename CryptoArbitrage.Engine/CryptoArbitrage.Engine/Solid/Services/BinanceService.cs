namespace ArbitrageProject.Services;

public class BinanceService : ExchangeService
{
    public BinanceService() : base("Binance") { }

    // Stub implementation for testing — replace with real API call later.
    public override decimal GetPrice(string symbol) =>
        symbol switch
        {
            "BTC" => 50000m,
            "ETH" => 3000m,
            _ => 1m
        };
}
