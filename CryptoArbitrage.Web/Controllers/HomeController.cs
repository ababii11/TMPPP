using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using ArbitrageProject.Interfaces;
using ArbitrageProject.Services;
using ArbitrageProject.Engines;
using ArbitrageProject.Strategies;
using CryptoArbitrage.Web.ViewModels;

namespace CryptoArbitrage.Web.Controllers;

public class HomeController : Controller
{
    private readonly IEnumerable<IExchangeService> _exchanges;

    public HomeController(IEnumerable<IExchangeService> exchanges)
    {
        _exchanges = exchanges;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(new ArbitrageFormViewModel());
    }

    [HttpPost]
    public IActionResult Index(ArbitrageFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Symbols))
        {
            model.Error = "Introdu simboluri (ex: BTC, ETH).";
            return View(model);
        }

        var symbols = model.Symbols
            .Split(new[] { ',', ';', ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim().ToUpperInvariant())
            .Distinct()
            .ToList();

        var priceProvider = new PriceProvider(_exchanges);
        var engine = new ArbitrageEngine(
            new SimpleArbitrageStrategy(model.MinProfitPercent),
            new FeeCalculator(model.FeePercent)
        );

        var results = new List<string>();

        foreach (var symbol in symbols)
        {
            var prices = priceProvider.GetPrices(symbol);
            if (prices.Count < 2) continue;

            var buy = prices.Min(p => p.Price);
            var sell = prices.Max(p => p.Price);

            var netPerUnit = engine.CheckOpportunity(buy, sell);
            if (netPerUnit > 0)
            {
                results.Add($"{symbol}: Buy @ {buy} -> Sell @ {sell} => Net/unit {netPerUnit:F2}");
            }
        }

        model.Opportunities = results;
        return View(model);
    }
}