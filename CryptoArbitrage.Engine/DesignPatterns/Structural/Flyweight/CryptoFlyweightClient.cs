namespace CryptoArbitrage.Engine.DesignPatterns.Structural.Flyweight;

/// <summary>
/// Client that creates contexts and uses factory-cached flyweights for crypto symbols.
/// </summary>
public class CryptoFlyweightClient
{
    private readonly List<MarketSymbolContext> _contexts = new();

    public string Run()
    {
        var factory = new FlyweightFactory();

        _contexts.Clear();

        _contexts.Add(new MarketSymbolContext(
            factory,
            "BTC",
            65010.75m,
            65190.20m,
            "Binance",
            "Kraken"));

        _contexts.Add(new MarketSymbolContext(
            factory,
            "btc",
            64980.10m,
            65120.55m,
            "Kraken",
            "Binance"));

        _contexts.Add(new MarketSymbolContext(
            factory,
            "ETH",
            3520.40m,
            3545.65m,
            "Binance",
            "Kraken"));

        var btcReused = ReferenceEquals(factory.GetFlyweight("BTC"), factory.GetFlyweight("btc"));

        var lines = _contexts.Select(c => c.Operation()).ToArray();

        return $@"Flyweight Client (Crypto)
{string.Join("\n", lines)}
BTC shared flyweight reused: {btcReused}
Factory cache size: {factory.CacheSize}";
    }
}
