using CryptoArbitrage.Engine.DesignPatterns.AbstractFactory;
using CryptoArbitrage.Engine.DesignPatterns.FactoryMethod;
using CryptoArbitrage.Engine.DesignPatterns.Structural.Composite;
using CryptoArbitrage.Engine.DesignPatterns.Structural.Facade;
using CryptoArbitrage.Engine.DesignPatterns.Structural.Adapter;

namespace CryptoArbitrage.Engine;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== CRYPTO ARBITRAGE DESIGN PATTERNS DEMO ===\n");

        
        Console.WriteLine("1. FACTORY METHOD PATTERN:");
        Console.WriteLine("-------------------------------");
        

        var profitCreator = new ProfitReportCreator(50000.25m, 50500.75m, 0.5m, "BTC/USD");
        var profitResult = profitCreator.ProcessAnalysis();
        Console.WriteLine(profitResult);
        Console.WriteLine();

        var riskCreator = new RiskReportCreator(12.5m, 7.2m, "Binance", "ETH/USD");
        var riskResult = riskCreator.ProcessAnalysis();
        Console.WriteLine(riskResult);
        Console.WriteLine();

        Console.WriteLine("2. ABSTRACT FACTORY PATTERN:");
        Console.WriteLine("--------------------------------");

        Console.WriteLine(">> BINANCE PLATFORM:");
        ITradingInstrumentsFactory binanceFactory = new BinanceTradingFactory();
        var binancePlatform = new TradingPlatform(binanceFactory);
        
        var binanceInit = binancePlatform.InitializePlatform();
        Console.WriteLine(binanceInit);
        Console.WriteLine();
        
        var binanceArbitrage = binancePlatform.ExecuteArbitrageStrategy("BTC/USDT");
        Console.WriteLine(binanceArbitrage);
        Console.WriteLine();

        Console.WriteLine(">> KRAKEN PLATFORM:");
        ITradingInstrumentsFactory krakenFactory = new KrakenTradingFactory();
        var krakenPlatform = new TradingPlatform(krakenFactory);
        
        var krakenInit = krakenPlatform.InitializePlatform();
        Console.WriteLine(krakenInit);
        Console.WriteLine();
        
        var krakenArbitrage = krakenPlatform.ExecuteArbitrageStrategy("ETH/EUR");
        Console.WriteLine(krakenArbitrage);

        Console.WriteLine();
        Console.WriteLine("3. ADAPTER PATTERN:");
        Console.WriteLine("-------------------------------");

        var legacyService = new LegacyExchangeTickerService();
        IExchangeMarketDataProvider adapter = new LegacyExchangeTickerAdapter(legacyService);
        var arbitrageClient = new ArbitrageBotClient(adapter);

        var normalizedTicker = arbitrageClient.ScanPair("BTC/USDT");
        Console.WriteLine($">> ADAPTER RESPONSE: {normalizedTicker}");

        Console.WriteLine();
        Console.WriteLine("4. COMPOSITE PATTERN:");
        Console.WriteLine("-------------------------------");

        var tradingWallet = new PortfolioGroup("Trading Wallet");
        tradingWallet.Add(new SpotPosition("BTC", 0.10m, 65000m));
        tradingWallet.Add(new SpotPosition("ETH", 1.20m, 3000m));

        var vault = new PortfolioGroup("Vault");
        vault.Add(new SpotPosition("SOL", 100m, 150m));

        var rootPortfolio = new PortfolioGroup("Global Portfolio");
        rootPortfolio.Add(tradingWallet);
        rootPortfolio.Add(vault);

        Console.WriteLine(rootPortfolio.Execute());

        Console.WriteLine();
        Console.WriteLine("5. FACADE PATTERN:");
        Console.WriteLine("-------------------------------");

        var executionFacade = new ArbitrageExecutionFacade(
            new OpportunityScannerService(),
            new LiquidityValidationService(),
            new OrderPlacementService(),
            new TradeAuditService(),
            new AdditionalArbitrageFacade());

        var facadeResult = executionFacade.SubsystemOperation("TRADER-01", "BTC/USDT", 0.2m);
        Console.WriteLine($">> {facadeResult}");

        Console.WriteLine("\n=== DEMO COMPLETED ===");
        Console.ReadKey();
    }
}