namespace CryptoArbitrage.Engine.DesignPatterns.Structural.Flyweight;

/// <summary>
/// Context keeps unique market state and references a shared flyweight instance.
/// </summary>
public class MarketSymbolContext
{
    private readonly decimal BuyPrice;
    private readonly decimal SellPrice;
    private readonly string ExchangeBuy;
    private readonly string ExchangeSell;
    private readonly Flyweight flyweight;

    public MarketSymbolContext(
        FlyweightFactory factory,
        string repeatingState,
        decimal buyPrice,
        decimal sellPrice,
        string exchangeBuy,
        string exchangeSell)
    {
        BuyPrice = buyPrice;
        SellPrice = sellPrice;
        ExchangeBuy = exchangeBuy;
        ExchangeSell = exchangeSell;
        flyweight = factory.GetFlyweight(repeatingState);
    }

    public string Operation()
    {
        return flyweight.Operation(BuyPrice, SellPrice, ExchangeBuy, ExchangeSell);
    }
}
