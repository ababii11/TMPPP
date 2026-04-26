namespace CryptoArbitrage.Engine.DesignPatterns.Structural.Decorator;

/// <summary>
/// ConcreteDecorator - adds logging around transaction processing.
/// </summary>
public class LoggingDecorator : CryptoOperationDecorator
{
    public LoggingDecorator(ICryptoOperation component) : base(component)
    {
    }

    public override string Process(string tradingPair, decimal amount)
    {
        Console.WriteLine($"[LOG] Start transaction. Pair={tradingPair}, Amount={amount:F4}");

        var result = base.Process(tradingPair, amount);

        LogTransaction();
        return result;
    }

    public void LogTransaction()
    {
        Console.WriteLine("[LOG] Transaction completed successfully.");
    }
}
