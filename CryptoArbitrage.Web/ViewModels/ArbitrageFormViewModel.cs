namespace CryptoArbitrage.Web.ViewModels;

public class ArbitrageFormViewModel
{
    public string Symbols { get; set; } = "BTC, ETH";
    public decimal FeePercent { get; set; } = 0.1m;
    public decimal MinProfitPercent { get; set; } = 0.5m;

    public List<OpportunityResult> Results { get; set; } = new();
    public string? Error { get; set; }
    public string? SummaryReport { get; set; }
    public string? PlatformStatus { get; set; }
}

public class OpportunityResult
{
    public string Symbol { get; set; } = "";
    public decimal BuyPrice { get; set; }
    public decimal SellPrice { get; set; }
    public string ExchangeBuy { get; set; } = "";
    public string ExchangeSell { get; set; } = "";
    public decimal NetPerUnit { get; set; }
    public bool IsProfitable { get; set; }
}
