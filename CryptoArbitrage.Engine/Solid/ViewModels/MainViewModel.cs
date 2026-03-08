using System.Collections.Generic;
using System.Linq;
using ArbitrageProject.Engines;
using ArbitrageProject.Interfaces;
using ArbitrageProject.Models;

namespace ArbitrageProject.ViewModels;

public class MainViewModel
{
    private readonly ArbitrageEngine _engine;
    private readonly IPriceProvider _provider;
    private readonly ITradeSimulation _simulation;

    public IReadOnlyList<string> OpportunityDescriptions { get; private set; } = new List<string>();

    public MainViewModel(ArbitrageEngine engine, IPriceProvider provider, ITradeSimulation simulation)
    {
        _engine = engine;
        _provider = provider;
        _simulation = simulation;
    }
    public void Refresh(IEnumerable<string> symbols)
    {
        var results = new List<string>();

        foreach (var symbol in symbols)
        {
            var prices = _provider.GetPrices(symbol);
            if (prices.Count < 2) continue;

            var buy = prices.Min(p => p.Price);
            var sell = prices.Max(p => p.Price);

            var netPerUnit = _engine.CheckOpportunity(buy, sell);
            if (netPerUnit > 0)
            {
                results.Add($"{symbol}: Buy @ {buy} -> Sell @ {sell} => Net/unit {netPerUnit:F2}");
            }
        }

        OpportunityDescriptions = results;
    }

    public decimal SimulateBestPair(string symbol, decimal investAmount)
    {
        var prices = _provider.GetPrices(symbol);
        if (prices.Count < 1) return 0m;

        var buyPrice = prices.OrderBy(p => p.Price).First();
        return _simulation.Simulate(buyPrice, investAmount);
    }
}