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
using CryptoArbitrage.Engine.DesignPatterns.Structural.Adapter;
using CryptoArbitrage.Engine.DesignPatterns.Structural.Composite;
using CryptoArbitrage.Engine.DesignPatterns.Structural.Facade;
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

        var firstResolved = results.FirstOrDefault(r => r.BuyPrice > 0 && r.SellPrice > 0);

        if (firstResolved != null)
        {
            var pair = $"{firstResolved.Symbol}/USDT";

            // Adapter: convert legacy exchange ticker format to a unified interface consumed by client.
            var adapterService = new LegacyExchangeTickerService();
            IExchangeMarketDataProvider adapter = new LegacyExchangeTickerAdapter(adapterService);
            var adapterClient = new ArbitrageBotClient(adapter);
            model.AdapterUnifiedTicker = adapterClient.ScanPair(pair);
            model.AdapterContext = $"Displayed opportunity combines {firstResolved.ExchangeBuy} and {firstResolved.ExchangeSell} prices for {firstResolved.Symbol}.";

            // Composite: group analyzed assets and execute uniformly through one component interface.
            var profitableGroup = new PortfolioGroup("Profitable Assets");
            var neutralGroup = new PortfolioGroup("Neutral/No Opportunity Assets");

            foreach (var item in results.Where(r => r.BuyPrice > 0))
            {
                var position = new SpotPosition(item.Symbol, 1m, item.BuyPrice);
                if (item.IsProfitable)
                    profitableGroup.Add(position);
                else
                    neutralGroup.Add(position);
            }

            var rootPortfolio = new PortfolioGroup("Analyzed Portfolio");
            rootPortfolio.Add(profitableGroup);
            rootPortfolio.Add(neutralGroup);
            model.CompositeStructure = rootPortfolio.Execute();

            // Facade: one operation orchestrates scanner, liquidity, placement and audit subsystems.
            var facade = new ArbitrageExecutionFacade(
                new OpportunityScannerService(),
                new LiquidityValidationService(),
                new OrderPlacementService(),
                new TradeAuditService(),
                new AdditionalArbitrageFacade());

            var amountFromPrice = Math.Round(100m / Math.Max(firstResolved.BuyPrice, 1m), 4);
            model.FacadeExecution = facade.SubsystemOperation("WEB-TRADER", pair, amountFromPrice);
        }

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
