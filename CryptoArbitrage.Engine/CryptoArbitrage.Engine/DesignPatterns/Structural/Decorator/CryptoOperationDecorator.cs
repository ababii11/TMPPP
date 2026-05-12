namespace CryptoArbitrage.Engine.DesignPatterns.Structural.Decorator;

/// <summary>
/// Decorator - base wrapper that delegates to a component.
/// </summary>
public abstract class CryptoOperationDecorator : ICryptoOperation
{
    private readonly ICryptoOperation _wrappee;

    protected CryptoOperationDecorator(ICryptoOperation component)
    {
        _wrappee = component;
    }

    public virtual string Process(string tradingPair, decimal amount)
    {
        return _wrappee.Process(tradingPair, amount);
    }
}
