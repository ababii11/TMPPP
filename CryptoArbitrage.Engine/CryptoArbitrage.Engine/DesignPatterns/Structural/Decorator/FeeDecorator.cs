namespace CryptoArbitrage.Engine.DesignPatterns.Structural.Decorator;

/// <summary>
/// ConcreteDecorator - applies a percentage-based fee to the transaction amount.
/// </summary>
public class FeeDecorator : CryptoOperationDecorator
{
    private readonly decimal _feeRate;

    public FeeDecorator(ICryptoOperation component, decimal feeRate) : base(component)
    {
        if (feeRate < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(feeRate), "Fee rate cannot be negative.");
        }

        _feeRate = feeRate;
    }

    public override string Process(string tradingPair, decimal amount)
    {
        var totalAmount = ApplyFee(amount);
        var feeAmount = totalAmount - amount;

        var baseResult = base.Process(tradingPair, totalAmount);
        return $"{baseResult} | Fee applied: {feeAmount:F4} ({_feeRate:P2})";
    }

    public decimal ApplyFee(decimal amount)
    {
        return amount + (amount * _feeRate);
    }
}
