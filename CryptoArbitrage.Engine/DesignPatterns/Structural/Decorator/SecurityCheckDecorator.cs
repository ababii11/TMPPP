namespace CryptoArbitrage.Engine.DesignPatterns.Structural.Decorator;

/// <summary>
/// ConcreteDecorator - validates inputs before delegating operation.
/// </summary>
public class SecurityCheckDecorator : CryptoOperationDecorator
{
    public SecurityCheckDecorator(ICryptoOperation component) : base(component)
    {
    }

    public override string Process(string tradingPair, decimal amount)
    {
        ValidateInput(tradingPair, amount);

        var baseResult = base.Process(tradingPair, amount);
        return $"{baseResult} | Security check: PASSED";
    }

    public void ValidateInput(string tradingPair, decimal amount)
    {
        if (string.IsNullOrWhiteSpace(tradingPair) || !tradingPair.Contains('/'))
        {
            throw new ArgumentException("Trading pair must be in format BASE/QUOTE (e.g., BTC/USDT).", nameof(tradingPair));
        }

        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than zero.");
        }
    }
}
