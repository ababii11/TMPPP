using System;
using System.Collections.Generic;
using System.Linq;
using ArbitrageProject.Engines;
using ArbitrageProject.Interfaces;
using ArbitrageProject.Models;

namespace CryptoArbitrage.Engine.DesignPatterns.Behavioral.Observer;

/// <summary>
/// ConcreteObserver that reacts when a profitable arbitrage opportunity is detected.
/// </summary>
public class ArbitrageBot : IPriceObserver
{
    private readonly ArbitrageEngine _engine;
    private readonly Action<string>? _messageSink;

    public string Name => "ArbitrageBot";

    public ArbitrageBot(ArbitrageEngine engine, Action<string>? messageSink = null)
    {
        _engine = engine;
        _messageSink = messageSink;
    }

    public void Update(string symbol, IReadOnlyList<CryptoPrice> prices)
    {
        if (prices.Count < 2)
        {
            Emit($"[{Name}] {symbol}: insufficient exchange prices.");
            return;
        }

        var buy = prices.MinBy(p => p.Price)!;
        var sell = prices.MaxBy(p => p.Price)!;

        var netPerUnit = _engine.CheckOpportunity(buy.Price, sell.Price);
        if (netPerUnit > 0m)
        {
            Emit(
                $"[{Name}] Opportunity {symbol}: BUY {buy.Price:N2} on {buy.Exchange} -> SELL {sell.Price:N2} on {sell.Exchange} | Net/unit {netPerUnit:N4}");
            return;
        }

        Emit($"[{Name}] {symbol}: no profitable opportunity.");
    }

    private void Emit(string message)
    {
        _messageSink?.Invoke(message);
        Console.WriteLine(message);
    }
}
