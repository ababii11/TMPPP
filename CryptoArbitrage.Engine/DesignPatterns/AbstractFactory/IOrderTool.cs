namespace CryptoArbitrage.Engine.DesignPatterns.AbstractFactory;

/// <summary>
/// AbstractProductA - Interface for order management tools
/// Equivalent to AbstractProductA from the diagram
/// </summary>
public interface IOrderTool
{
    /// <summary>
    /// Places a buy order on the exchange
    /// </summary>
    string PlaceBuyOrder(string cryptoPair, decimal amount, decimal price);
    
    /// <summary>
    /// Places a sell order on the exchange
    /// </summary>
    string PlaceSellOrder(string cryptoPair, decimal amount, decimal price);
    
    /// <summary>
    /// Cancels an existing order
    /// </summary>
    string CancelOrder(string orderId);
    
    /// <summary>
    /// Gets the exchange name this tool operates on
    /// </summary>
    string ExchangeName { get; }
}