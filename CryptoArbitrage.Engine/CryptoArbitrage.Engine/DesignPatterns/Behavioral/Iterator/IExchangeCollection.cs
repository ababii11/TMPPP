namespace CryptoArbitrage.Engine.DesignPatterns.Behavioral.Iterator;

/// <summary>
/// Aggregate interface that creates an iterator over exchange services.
/// </summary>
public interface IExchangeCollection
{
    IIterator CreateIterator();
}
