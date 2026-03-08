namespace CryptoArbitrage.Engine.DesignPatterns.AbstractFactory;

/// <summary>
/// Client - Uses the abstract factory to create families of related products
/// Equivalent to Client from the diagram
/// </summary>
public class TradingPlatform
{
    private readonly ITradingInstrumentsFactory _factory;
    private IOrderTool? _orderTool;
    private IAnalyticsTool? _analyticsTool;

    /// <summary>
    /// Client constructor - receives the factory (equivalent to factory: AbstractFactory from diagram)
    /// </summary>
    public TradingPlatform(ITradingInstrumentsFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Equivalent to someOperation() from diagram - uses the products created by factory
    /// Implementation of: ProductA pa = factory.createProductA(); ProductB pb = factory.createProductB();
    /// </summary>
    public string InitializePlatform()
    {
        // Equivalent to: ProductA pa = factory.createProductA();
        _orderTool = _factory.CreateOrderTool();
        
        // Equivalent to: ProductB pb = factory.createProductB(); 
        _analyticsTool = _factory.CreateAnalyticsTool();

        return $@"Trading Platform Initialized:
Exchange: {_orderTool.ExchangeName}
Order Tool: {_orderTool.GetType().Name}
Analytics Tool: {_analyticsTool.GetType().Name}
Status: READY FOR TRADING";
    }

    /// <summary>
    /// Demonstrates using the created products together
    /// </summary>
    public string ExecuteArbitrageStrategy(string cryptoPair)
    {
        if (_orderTool == null || _analyticsTool == null)
        {
            return "ERROR: Platform not initialized. Call InitializePlatform() first.";
        }

        var analysis = _analyticsTool.AnalyzeMarketTrends(cryptoPair);
        var buyOrder = _orderTool.PlaceBuyOrder(cryptoPair, 0.1m, 45000m);
        var sellOrder = _orderTool.PlaceSellOrder(cryptoPair, 0.1m, 45200m);

        return $@"=== ARBITRAGE EXECUTION ===
{analysis}

{buyOrder}

{sellOrder}
============================";
    }
}