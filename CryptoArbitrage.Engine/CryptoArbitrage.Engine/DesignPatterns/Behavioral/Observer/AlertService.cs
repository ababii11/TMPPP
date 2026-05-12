using System;
using System.Collections.Generic;
using System.Linq;
using ArbitrageProject.Models;

namespace CryptoArbitrage.Engine.DesignPatterns.Behavioral.Observer;

/// <summary>
/// ConcreteObserver that raises alerts when spread exceeds configured threshold.
/// </summary>
public class AlertService : IPriceObserver
{
    private readonly decimal _spreadAlertPercent;
    private readonly Action<string>? _messageSink;

    public string Name => "AlertService";

    public AlertService(decimal spreadAlertPercent = 0.40m, Action<string>? messageSink = null)
    {
        _spreadAlertPercent = spreadAlertPercent;
        _messageSink = messageSink;
    }

    public void Update(string symbol, IReadOnlyList<CryptoPrice> prices)
    {
        if (prices.Count < 2)
        {
            return;
        }

        var low = prices.MinBy(p => p.Price)!;
        var high = prices.MaxBy(p => p.Price)!;

        if (low.Price <= 0m)
        {
            return;
        }

        var spreadPercent = (high.Price - low.Price) / low.Price * 100m;
        if (spreadPercent >= _spreadAlertPercent)
        {
            var message = $"[{Name}] ALERT {symbol}: spread {spreadPercent:N3}% between {low.Exchange} and {high.Exchange}.";
            _messageSink?.Invoke(message);
            Console.WriteLine(message);
        }
    }
}
