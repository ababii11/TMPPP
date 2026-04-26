namespace ArbitrageProject.Models;

public class ArbitrageOpportunity
{
    public string StrategyName { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public bool IsProfitable { get; set; }
    public decimal BuyPrice { get; set; }
    public decimal SellPrice { get; set; }
    public string ExchangeBuy { get; set; } = string.Empty;
    public string ExchangeSell { get; set; } = string.Empty;
    public decimal GrossPerUnit { get; set; }
    public decimal NetPerUnit { get; set; }
    public string RouteDescription { get; set; } = string.Empty;
}