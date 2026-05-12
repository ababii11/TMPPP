namespace CryptoArbitrage.Engine.DesignPatterns.Structural.Decorator;

/// <summary>
/// ConcreteComponent - basic transaction processing behavior.
/// </summary>
public class BasicTransactionService : ICryptoOperation
{
    public string Process(string tradingPair, decimal amount)
    {
        return $"Transaction executed: Pair={tradingPair}, Amount={amount:F4}";
    }
}
