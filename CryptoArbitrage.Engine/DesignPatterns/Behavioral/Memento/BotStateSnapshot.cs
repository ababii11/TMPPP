namespace CryptoArbitrage.Engine.DesignPatterns.Behavioral.Memento;

/// <summary>
/// Memento object - immutable snapshot of bot runtime state.
/// </summary>
public class BotStateSnapshot
{
    public string SnapshotId { get; init; } = Guid.NewGuid().ToString("N");
    public DateTime CapturedAt { get; init; } = DateTime.UtcNow;
    public string Label { get; init; } = string.Empty;

    public decimal Balance { get; init; }
    public string BalanceCurrency { get; init; } = "USDT";
    public string WalletName { get; init; } = "Primary wallet";
    public string ActiveStrategy { get; init; } = "SimpleArbitrageStrategy";

    public IReadOnlyList<WalletTransactionEntry> WalletHistory { get; init; } = new List<WalletTransactionEntry>();
    public IReadOnlyList<BotTradeRecord> OpenTrades { get; init; } = new List<BotTradeRecord>();
    public IReadOnlyList<BotTradeRecord> TradeHistory { get; init; } = new List<BotTradeRecord>();
}
