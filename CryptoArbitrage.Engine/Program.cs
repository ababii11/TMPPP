using CryptoArbitrage.Engine.DesignPatterns.AbstractFactory;
using CryptoArbitrage.Engine.DesignPatterns.FactoryMethod;

namespace CryptoArbitrage.Engine;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== CRYPTO ARBITRAGE DESIGN PATTERNS DEMO ===\n");

        // Factory Method Demo
        Console.WriteLine("1. FACTORY METHOD PATTERN:");
        Console.WriteLine("-------------------------------");
        
        // Profit Report
        var profitCreator = new ProfitReportCreator(50000.25m, 50500.75m, 0.5m, "BTC/USD");
        var profitResult = profitCreator.ProcessAnalysis();
        Console.WriteLine(profitResult);
        Console.WriteLine();

        // Risk Report
        var riskCreator = new RiskReportCreator(12.5m, 7.2m, "Binance", "ETH/USD");
        var riskResult = riskCreator.ProcessAnalysis();
        Console.WriteLine(riskResult);
        Console.WriteLine();

        // Abstract Factory Demo
        Console.WriteLine("2. ABSTRACT FACTORY PATTERN:");
        Console.WriteLine("--------------------------------");

        // Binance Platform
        Console.WriteLine(">> BINANCE PLATFORM:");
        ITradingInstrumentsFactory binanceFactory = new BinanceTradingFactory();
        var binancePlatform = new TradingPlatform(binanceFactory);
        
        var binanceInit = binancePlatform.InitializePlatform();
        Console.WriteLine(binanceInit);
        Console.WriteLine();
        
        var binanceArbitrage = binancePlatform.ExecuteArbitrageStrategy("BTC/USDT");
        Console.WriteLine(binanceArbitrage);
        Console.WriteLine();

        // Kraken Platform
        Console.WriteLine(">> KRAKEN PLATFORM:");
        ITradingInstrumentsFactory krakenFactory = new KrakenTradingFactory();
        var krakenPlatform = new TradingPlatform(krakenFactory);
        
        var krakenInit = krakenPlatform.InitializePlatform();
        Console.WriteLine(krakenInit);
        Console.WriteLine();
        
        var krakenArbitrage = krakenPlatform.ExecuteArbitrageStrategy("ETH/EUR");
        Console.WriteLine(krakenArbitrage);

        Console.WriteLine("\n=== DEMO COMPLETED ===");
        Console.ReadKey();
    }
}