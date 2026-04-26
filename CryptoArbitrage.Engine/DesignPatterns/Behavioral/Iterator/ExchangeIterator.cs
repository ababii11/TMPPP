using ArbitrageProject.Interfaces;

namespace CryptoArbitrage.Engine.DesignPatterns.Behavioral.Iterator;

/// <summary>
/// Concrete iterator for sequential traversal of exchange services.
/// </summary>
public class ExchangeIterator : IIterator
{
    private readonly IReadOnlyList<IExchangeService> _exchanges;
    private int _position;

    public ExchangeIterator(IReadOnlyList<IExchangeService> exchanges)
    {
        _exchanges = exchanges;
        _position = 0;
    }

    public bool HasNext()
    {
        return _position < _exchanges.Count;
    }

    public IExchangeService Next()
    {
        if (!HasNext())
        {
            throw new InvalidOperationException("No more exchange services in iterator.");
        }

        return _exchanges[_position++];
    }

    public void Reset()
    {
        _position = 0;
    }
}
