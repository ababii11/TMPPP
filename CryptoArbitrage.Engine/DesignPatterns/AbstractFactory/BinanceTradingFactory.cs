namespace CryptoArbitrage.Engine.DesignPatterns.AbstractFactory;

/// <summary>
/// ConcreteFactory1 - Binance implementation of trading instruments factory
/// Equivalent to ConcreteFactory1 from the diagram
/// </summary>
public class BinanceTradingFactory : ITradingInstrumentsFactory
{
    /// <summary>
    /// Creates Binance-specific order tool
    /// Equivalent to createProductA(): ProductA from diagram
    /// Implementation: return new ConcreteProductA1()
    /// </summary>
    public IOrderTool CreateOrderTool()
    {
        return new BinanceOrderTool();
    }

    /// <summary>
    /// Creates Binance-specific analytics tool
    /// Equivalent to createProductB(): ProductB from diagram
    /// Implementation: return new ConcreteProductB1()
    /// </summary>
    public IAnalyticsTool CreateAnalyticsTool()
    {
        return new BinanceAnalyticsTool();
    }
}