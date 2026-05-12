namespace CryptoArbitrage.Engine.DesignPatterns.Behavioral.Memento;

public class WalletTransactionEntry
{
    public DateTime Timestamp { get; set; }
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public string CryptoType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Pending";
}
