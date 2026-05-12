namespace CryptoArbitrage.Engine.DesignPatterns.Structural.Decorator;

/// <summary>
/// Simple usage example showing dynamic decorator chaining.
/// </summary>
public static class DecoratorUsageExample
{
    public static string Run()
    {
        ICryptoOperation operation = new BasicTransactionService();

        operation = new LoggingDecorator(operation);
        operation = new FeeDecorator(operation, 0.0025m);
        operation = new SecurityCheckDecorator(operation);

        return operation.Process("BTC/USDT", 0.1500m);
    }
}
