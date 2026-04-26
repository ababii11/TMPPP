namespace CryptoArbitrage.Web.Models;

public class TradeCommandRequest
{
    public string Exchange { get; set; } = "binance";
    public string Side { get; set; } = "buy";
    public string Pair { get; set; } = "BTC/USDT";
    public decimal Amount { get; set; }
    public decimal Price { get; set; }
}
