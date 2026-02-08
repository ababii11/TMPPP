namespace ArbitrageProject.Services;

public class KrakenService : ExchangeService
{
    public KrakenService() : base("Kraken") { }

    // Stub implementation for testing — replace with real API call later.
    public override decimal GetPrice(string symbol) =>
        symbol switch
        {
            "BTC" => 50100m,
            "ETH" => 2980m,
            _ => 1m
        };
}
