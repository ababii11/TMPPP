using ArbitrageProject.Interfaces;

namespace CryptoArbitrage.Engine.DesignPatterns.Behavioral.Iterator;

/// <summary>
/// Iterator interface for traversing exchange services without exposing collection internals.
/// </summary>
public interface IIterator
{
    bool HasNext();

    IExchangeService Next();

    void Reset();
}
