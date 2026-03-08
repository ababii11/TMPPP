using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using ArbitrageProject.Interfaces;
using ArbitrageProject.Services;
using ArbitrageProject.Engines;
using ArbitrageProject.Strategies;
using CryptoArbitrage.Engine.DesignPatterns.FactoryMethod;
using CryptoArbitrage.Engine.DesignPatterns.AbstractFactory;
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

        // SOLID: PriceProvider, ArbitrageEngine, IArbitrageStrategy, IFeeCalculator
        var priceProvider = new PriceProvider(_exchanges);
        var engine = new ArbitrageEngine(
            new SimpleArbitrageStrategy(model.MinProfitPercent),
            new FeeCalculator(model.FeePercent)
        );

        var results = new List<OpportunityResult>();
        OpportunityResult? firstProfitable = null;

        foreach (var symbol in symbols)
        {
            var prices = priceProvider.GetPrices(symbol);
            if (prices.Count < 2)
            {
                results.Add(new OpportunityResult { Symbol = symbol, IsProfitable = false });
                continue;
            }

            var minPrice = prices.MinBy(p => p.Price)!;
            var maxPrice = prices.MaxBy(p => p.Price)!;
            var buy = minPrice.Price;
            var sell = maxPrice.Price;

            var netPerUnit = engine.CheckOpportunity(buy, sell);
            var isProfitable = netPerUnit > 0;

            var item = new OpportunityResult
            {
                Symbol = symbol,
                BuyPrice = buy,
                SellPrice = sell,
                ExchangeBuy = minPrice.Exchange,
                ExchangeSell = maxPrice.Exchange,
                NetPerUnit = netPerUnit,
                IsProfitable = isProfitable
            };
            results.Add(item);
            if (isProfitable && firstProfitable == null)
                firstProfitable = item;
        }

        model.Results = results;

        // Factory Method: ProfitReportCreator → ProfitAnalysisReport
        if (firstProfitable != null)
        {
            var reportCreator = new ProfitReportCreator(
                firstProfitable.BuyPrice, firstProfitable.SellPrice, 1m, firstProfitable.Symbol);
            model.SummaryReport = reportCreator.ProcessAnalysis();
        }

        // Abstract Factory: TradingPlatform + BinanceTradingFactory
        var factory = new BinanceTradingFactory();
        var platform = new TradingPlatform(factory);
        model.PlatformStatus = platform.InitializePlatform();

        return View(model);
    }
}
