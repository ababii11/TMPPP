using System;
using System.Collections.Generic;
using System.Linq;
using ArbitrageProject.Models;

namespace CryptoArbitrage.Engine.DesignPatterns.Behavioral.Observer;

/// <summary>
/// ConcreteObserver that logs each incoming market update.
/// </summary>
public class LoggerService : IPriceObserver
{
    private readonly Action<string>? _messageSink;

    public string Name => "LoggerService";

    public LoggerService(Action<string>? messageSink = null)
    {
        _messageSink = messageSink;
    }

    public void Update(string symbol, IReadOnlyList<CryptoPrice> prices)
    {
        var logLine = string.Join(" | ", prices.Select(p => $"{p.Exchange}:{p.Price:N2}"));
        var message = $"[{Name}] Tick {symbol}: {logLine}";
        _messageSink?.Invoke(message);
        Console.WriteLine(message);
    }
}
