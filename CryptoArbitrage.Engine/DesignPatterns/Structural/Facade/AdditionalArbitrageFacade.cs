namespace CryptoArbitrage.Engine.DesignPatterns.Structural.Facade;

/// <summary>
/// Optional additional facade used for extra controls over subsystem flow.
/// </summary>
public class AdditionalArbitrageFacade
{
    public bool AnotherOperation(string pair, decimal amount)
    {
        return !string.IsNullOrWhiteSpace(pair) && amount <= 1.5m;
    }
}
