namespace CryptoArbitrage.Engine.DesignPatterns.Behavioral.Memento;

public class BotTradeRecord
{
    public DateTime Timestamp { get; set; }
    public string OrderId { get; set; } = string.Empty;
    public string Exchange { get; set; } = string.Empty;
    public string Side { get; set; } = string.Empty;
    public string Pair { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Price { get; set; }
    public string Status { get; set; } = "Open";
    public string ExecutionPayload { get; set; } = string.Empty;
}
