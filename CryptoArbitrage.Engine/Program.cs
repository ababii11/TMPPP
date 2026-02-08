using System;
using System.Collections.Generic;
using ArbitrageProject.Interfaces;
using ArbitrageProject.Services;
using ArbitrageProject.Strategies;
using ArbitrageProject.Engines;
using ArbitrageProject.ViewModels;

internal class Program
{
    private static void Main()
    {
        // Compose using interfaces (simple manual DI)
        var exchanges = new List<IExchangeService>
        {
            new BinanceService(),
            new KrakenService()
        };

        var priceProvider = new PriceProvider(exchanges);
        var feeCalculator = new FeeCalculator(0.1m);          // fee percent
        var strategy = new SimpleArbitrageStrategy(0.5m);     // minimal profit percent

        var engine = new ArbitrageEngine(strategy, feeCalculator);
        var vm = new MainViewModel(engine, priceProvider);

        vm.Refresh(new[] { "BTC", "ETH" });

        foreach (var desc in vm.OpportunityDescriptions)
            Console.WriteLine(desc);
    }
}