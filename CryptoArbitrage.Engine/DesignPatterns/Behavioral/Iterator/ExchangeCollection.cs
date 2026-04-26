using ArbitrageProject.Interfaces;

namespace CryptoArbitrage.Engine.DesignPatterns.Behavioral.Iterator;

/// <summary>
/// Concrete collection that wraps real exchange services used in arbitrage logic.
/// </summary>
public class ExchangeCollection : IExchangeCollection
{
    private readonly List<IExchangeService> _exchanges;

    public ExchangeCollection(IEnumerable<IExchangeService> exchanges)
    {
        _exchanges = exchanges.ToList();
    }

    public IIterator CreateIterator()
    {
        return new ExchangeIterator(_exchanges);
    }
}
