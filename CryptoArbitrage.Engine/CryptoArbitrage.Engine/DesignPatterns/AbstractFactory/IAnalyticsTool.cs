namespace CryptoArbitrage.Engine.DesignPatterns.AbstractFactory;

/// <summary>
/// AbstractProductB - Interface for analytics tools
/// Equivalent to AbstractProductB from the diagram
/// </summary>
public interface IAnalyticsTool
{
    /// <summary>
    /// Analyzes market trends for a crypto pair
    /// </summary>
    string AnalyzeMarketTrends(string cryptoPair);
    
    /// <summary>
    /// Calculates volatility metrics
    /// </summary>
    decimal CalculateVolatility(string cryptoPair, int periodDays);
    
    /// <summary>
    /// Generates trading signals
    /// </summary>
    string GenerateTradingSignals(string cryptoPair);
    
    /// <summary>
    /// Gets the exchange name this analytics tool operates on
    /// </summary>
    string ExchangeName { get; }
}