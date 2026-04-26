namespace CryptoArbitrage.Engine.DesignPatterns.Behavioral.Strategy;

/// <summary>
/// Common result object returned by all concrete strategies.
/// </summary>
public class StrategyEvaluationResult
{
    public string StrategyName { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public bool IsProfitable { get; set; }
    public decimal BuyPrice { get; set; }
    public decimal SellPrice { get; set; }
    public string BuyExchange { get; set; } = string.Empty;
    public string SellExchange { get; set; } = string.Empty;
    public decimal GrossProfitPerUnit { get; set; }
    public decimal NetProfitPerUnit { get; set; }
    public string Route { get; set; } = string.Empty;
}
