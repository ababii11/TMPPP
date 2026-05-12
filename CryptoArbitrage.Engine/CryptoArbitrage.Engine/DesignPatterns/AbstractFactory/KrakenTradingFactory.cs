namespace CryptoArbitrage.Engine.DesignPatterns.AbstractFactory;

/// <summary>
/// ConcreteFactory2 - Kraken implementation of trading instruments factory
/// Equivalent to ConcreteFactory2 from the diagram
/// </summary>
public class KrakenTradingFactory : ITradingInstrumentsFactory
{
    /// <summary>
    /// Creates Kraken-specific order tool
    /// Equivalent to createProductA(): ProductA from diagram
    /// Implementation: return new ConcreteProductA2()
    /// </summary>
    public IOrderTool CreateOrderTool()
    {
        return new KrakenOrderTool();
    }

    /// <summary>
    /// Creates Kraken-specific analytics tool
    /// Equivalent to createProductB(): ProductB from diagram
    /// Implementation: return new ConcreteProductB2()
    /// </summary>
    public IAnalyticsTool CreateAnalyticsTool()
    {
        return new KrakenAnalyticsTool();
    }
}