namespace CryptoArbitrage.Web.Models;

public class WalletTransactionRequest
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CryptoType { get; set; } = string.Empty;
}
