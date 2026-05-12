namespace CryptoArbitrage.Engine.DesignPatterns.Structural.Decorator;

/// <summary>
/// Component - common interface for crypto operations.
/// </summary>
public interface ICryptoOperation
{
    string Process(string tradingPair, decimal amount);
}
