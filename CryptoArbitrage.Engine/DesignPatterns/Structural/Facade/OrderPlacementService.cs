namespace CryptoArbitrage.Engine.DesignPatterns.Structural.Facade;

public class OrderPlacementService
{
    public string PlaceArbitrageOrder(string traderId, string pair, decimal amount)
    {
        var seed = Math.Abs(HashCode.Combine(traderId, pair, amount));
        return $"ARB-{seed % 1_000_000:D6}";
    }
}
