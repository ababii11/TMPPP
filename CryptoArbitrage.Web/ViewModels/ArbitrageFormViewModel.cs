using System.Collections.Generic;

namespace CryptoArbitrage.Web.ViewModels;

public class ArbitrageFormViewModel
{
    public string Symbols { get; set; } = "BTC, ETH";
    public decimal FeePercent { get; set; } = 0.1m;
    public decimal MinProfitPercent { get; set; } = 0.5m;

    public List<string> Opportunities { get; set; } = new();
    public string? Error { get; set; }
}