using System;
using System.Collections.Generic;
using ArbitrageProject.Interfaces;
using ArbitrageProject.Models;
using CryptoArbitrage.Engine.DesignPatterns.Behavioral.Iterator;

namespace CryptoArbitrage.Engine.DesignPatterns.Behavioral.Observer;

/// <summary>
/// ConcreteSubject that simulates live exchange prices and notifies all observers.
/// </summary>
public class CryptoPriceService : IPriceSubject
{
    private readonly List<IPriceObserver> _observers = new();
    private readonly IEnumerable<IExchangeService> _exchanges;
    private readonly Dictionary<string, IReadOnlyList<CryptoPrice>> _latestBySymbol = new();
    private readonly Random _random;

    public CryptoPriceService(IEnumerable<IExchangeService> exchanges, int? seed = null)
    {
        _exchanges = exchanges;
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    public void Attach(IPriceObserver observer)
    {
        if (_observers.Contains(observer))
        {
            return;
        }

        _observers.Add(observer);
    }

    public void Detach(IPriceObserver observer)
    {
        _observers.Remove(observer);
    }

    public void Notify(string symbol)
    {
        if (!_latestBySymbol.TryGetValue(symbol, out var prices))
        {
            return;
        }

        foreach (var observer in _observers)
        {
            observer.Update(symbol, prices);
        }
    }

    public IReadOnlyList<CryptoPrice> SimulatePriceTick(string symbol)
    {
        var collection = new ExchangeCollection(_exchanges);
        var iterator = collection.CreateIterator();
        var updated = new List<CryptoPrice>();

        while (iterator.HasNext())
        {
            var exchange = iterator.Next();
            var basePrice = exchange.GetPrice(symbol);
            var drift = 1m + RandomPercent(-0.012m, 0.012m);
            var simulated = Math.Round(basePrice * drift, 2);
            updated.Add(new CryptoPrice(symbol, simulated, exchange.GetName()));
        }

        _latestBySymbol[symbol] = updated;
        Notify(symbol);
        return updated;
    }

    private decimal RandomPercent(decimal min, decimal max)
    {
        var sample = (decimal)_random.NextDouble();
        return min + (max - min) * sample;
    }
}
