namespace CryptoArbitrage.Engine.DesignPatterns.Behavioral.Memento;

/// <summary>
/// Caretaker that stores and restores bot mementos.
/// </summary>
public class StateManager
{
    private readonly object _sync = new();
    private readonly List<BotStateSnapshot> _snapshots = new();

    public void LoadSnapshots(IEnumerable<BotStateSnapshot> snapshots)
    {
        lock (_sync)
        {
            _snapshots.Clear();
            _snapshots.AddRange(snapshots.OrderByDescending(s => s.CapturedAt));
        }
    }

    public BotStateSnapshot Save(ArbitrageBotStateOriginator originator, string label)
    {
        lock (_sync)
        {
            var snapshot = originator.SaveToMemento(label);
            _snapshots.Insert(0, snapshot);
            return snapshot;
        }
    }

    public bool Restore(ArbitrageBotStateOriginator originator, string snapshotId)
    {
        lock (_sync)
        {
            var snapshot = _snapshots.FirstOrDefault(s => s.SnapshotId == snapshotId);
            if (snapshot == null)
            {
                return false;
            }

            originator.RestoreFromMemento(snapshot);
            return true;
        }
    }

    public IReadOnlyList<object> GetSnapshots()
    {
        lock (_sync)
        {
            return _snapshots.Select(s => (object)new
            {
                snapshotId = s.SnapshotId,
                capturedAt = s.CapturedAt.ToString("yyyy-MM-dd HH:mm:ss") + " UTC",
                label = s.Label,
                activeStrategy = s.ActiveStrategy,
                balance = s.Balance,
                openTrades = s.OpenTrades.Count,
                tradeHistory = s.TradeHistory.Count
            }).ToList();
        }
    }
}
