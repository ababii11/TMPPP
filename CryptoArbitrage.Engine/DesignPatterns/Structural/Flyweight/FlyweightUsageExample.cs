namespace CryptoArbitrage.Engine.DesignPatterns.Structural.Flyweight;

/// <summary>
/// Entry point wrapper for running the Flyweight client demo.
/// </summary>
public static class FlyweightUsageExample
{
    public static string Run()
    {
        var client = new CryptoFlyweightClient();
        return client.Run();
    }
}
