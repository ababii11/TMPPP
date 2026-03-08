namespace CryptoArbitrage.Engine.DesignPatterns.AbstractFactory;

/// <summary>
/// AbstractFactory - Interface for creating families of trading instruments
/// Equivalent to AbstractFactory from the diagram
/// </summary>
public interface ITradingInstrumentsFactory
{
    /// <summary>
    /// Creates an order tool - equivalent to createProductA(): ProductA from diagram
    /// </summary>
    IOrderTool CreateOrderTool();
    
    /// <summary>
    /// Creates an analytics tool - equivalent to createProductB(): ProductB from diagram
    /// </summary>
    IAnalyticsTool CreateAnalyticsTool();
}