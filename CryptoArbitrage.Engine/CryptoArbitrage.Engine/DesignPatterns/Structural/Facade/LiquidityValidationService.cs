namespace CryptoArbitrage.Engine.DesignPatterns.Structural.Facade;

public class LiquidityValidationService
{
    public bool HasSufficientLiquidity(string pair, decimal amount)
    {
        return !string.IsNullOrWhiteSpace(pair) && amount > 0m && amount <= 5m;
    }
}
