namespace CryptoArbitrage.Engine.DesignPatterns.Structural.Bridge;

/// <summary>
/// Abstraction Class - CryptoOperation
/// Defines the high-level interface for crypto operations.
/// Holds a reference to an ICryptoImplementor (the bridge).
/// This allows abstraction and implementation to vary independently.
/// </summary>
public abstract class CryptoOperation
{
    /// <summary>
    /// Reference to the implementor - this is the BRIDGE.
    /// </summary>
    protected ICryptoImplementor Implementor { get; set; }

    protected CryptoOperation(ICryptoImplementor implementor)
    {
        Implementor = implementor ?? throw new ArgumentNullException(nameof(implementor));
    }

    /// <summary>
    /// High-level operation that delegates to the implementor.
    /// </summary>
    public abstract void Execute();

    /// <summary>
    /// Gets information about the current operation and its implementation.
    /// </summary>
    public virtual string GetOperationInfo()
    {
        return $"Operation: {GetType().Name}, Implementation: {Implementor.GetImplementationName()}";
    }

    /// <summary>
    /// Allows switching implementations at runtime.
    /// </summary>
    public void SwitchImplementor(ICryptoImplementor newImplementor)
    {
        Implementor = newImplementor ?? throw new ArgumentNullException(nameof(newImplementor));
        Console.WriteLine($"  [CryptoOperation] Switched to: {Implementor.GetImplementationName()}");
    }
}
