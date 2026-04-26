using System.Text.RegularExpressions;

namespace CryptoArbitrage.Engine.DesignPatterns.Behavioral.Memento;

/// <summary>
/// Originator that owns runtime bot state and can create/restore mementos.
/// </summary>
public class ArbitrageBotStateOriginator
{
    private readonly object _sync = new();

    private decimal _balance = 12500m;
    private string _balanceCurrency = "USDT";
    private string _walletName = "Primary wallet";
    private string _activeStrategy = "SimpleArbitrageStrategy";

    private List<WalletTransactionEntry> _walletHistory =
    [
        new WalletTransactionEntry
        {
            Timestamp = DateTime.UtcNow.AddMinutes(-40),
            From = "0x01A",
            To = "0x09F",
            CryptoType = "BTC",
            Amount = 0.0421m,
            Status = "Completed"
        },
        new WalletTransactionEntry
        {
            Timestamp = DateTime.UtcNow.AddMinutes(-15),
            From = "0x01A",
            To = "0x19B",
            CryptoType = "ETH",
            Amount = 0.3100m,
            Status = "Completed"
        }
    ];

    private List<BotTradeRecord> _openTrades = new();
    private List<BotTradeRecord> _tradeHistory = new();

    public object GetBalance()
    {
        lock (_sync)
        {
            return new
            {
                balance = _balance,
                currency = _balanceCurrency,
                walletName = _walletName
            };
        }
    }

    public object AddWalletTransaction(string from, string to, decimal amount, string cryptoType)
    {
        lock (_sync)
        {
            var entry = new WalletTransactionEntry
            {
                Timestamp = DateTime.UtcNow,
                From = from,
                To = to,
                CryptoType = cryptoType,
                Amount = amount,
                Status = "Completed"
            };

            var conversion = cryptoType.ToUpperInvariant() switch
            {
                "BTC" => 64000m,
                "ETH" => 3200m,
                "SOL" => 145m,
                "ADA" => 0.65m,
                _ => 100m
            };

            _balance -= amount * conversion;
            if (_balance < 0m)
            {
                _balance = 0m;
            }

            _walletHistory.Insert(0, entry);

            return new
            {
                success = true,
                message = "Transaction recorded",
                transactionId = Guid.NewGuid().ToString("N"),
                status = entry.Status
            };
        }
    }

    public IReadOnlyList<object> GetWalletHistory()
    {
        lock (_sync)
        {
            return _walletHistory.Select(x => (object)new
            {
                timestamp = x.Timestamp.ToString("yyyy-MM-dd HH:mm:ss") + " UTC",
                from = x.From,
                to = x.To,
                cryptoType = x.CryptoType,
                amount = x.Amount,
                status = x.Status
            }).ToList();
        }
    }

    public void SetActiveStrategy(string strategyName)
    {
        lock (_sync)
        {
            _activeStrategy = string.IsNullOrWhiteSpace(strategyName)
                ? _activeStrategy
                : strategyName;
        }
    }

    public void RecordTradeExecution(
        string exchange,
        string side,
        string pair,
        decimal amount,
        decimal price,
        string executionPayload)
    {
        lock (_sync)
        {
            var orderId = ExtractOrderId(executionPayload) ?? $"ORD-{Guid.NewGuid():N}";

            _openTrades.Insert(0, new BotTradeRecord
            {
                Timestamp = DateTime.UtcNow,
                OrderId = orderId,
                Exchange = exchange,
                Side = side,
                Pair = pair,
                Amount = amount,
                Price = price,
                Status = "Open",
                ExecutionPayload = executionPayload
            });
        }
    }

    public void RecordUndo(string undoPayload)
    {
        lock (_sync)
        {
            if (_openTrades.Count == 0)
            {
                return;
            }

            var trade = _openTrades[0];
            _openTrades.RemoveAt(0);
            trade.Status = "Canceled";
            trade.ExecutionPayload = undoPayload;
            _tradeHistory.Insert(0, trade);
        }
    }

    public object GetBotState()
    {
        lock (_sync)
        {
            return new
            {
                balance = _balance,
                currency = _balanceCurrency,
                walletName = _walletName,
                activeStrategy = _activeStrategy,
                openTrades = _openTrades.Select(MapTrade).ToList(),
                tradeHistory = _tradeHistory.Select(MapTrade).ToList(),
                walletHistory = _walletHistory.Select(MapWallet).ToList()
            };
        }
    }

    public BotStateSnapshot SaveToMemento(string label)
    {
        lock (_sync)
        {
            return new BotStateSnapshot
            {
                SnapshotId = Guid.NewGuid().ToString("N"),
                CapturedAt = DateTime.UtcNow,
                Label = label,
                Balance = _balance,
                BalanceCurrency = _balanceCurrency,
                WalletName = _walletName,
                ActiveStrategy = _activeStrategy,
                WalletHistory = CloneWalletHistory(_walletHistory),
                OpenTrades = CloneTradeHistory(_openTrades),
                TradeHistory = CloneTradeHistory(_tradeHistory)
            };
        }
    }

    public void RestoreFromMemento(BotStateSnapshot snapshot)
    {
        lock (_sync)
        {
            _balance = snapshot.Balance;
            _balanceCurrency = snapshot.BalanceCurrency;
            _walletName = snapshot.WalletName;
            _activeStrategy = snapshot.ActiveStrategy;
            _walletHistory = CloneWalletHistory(snapshot.WalletHistory);
            _openTrades = CloneTradeHistory(snapshot.OpenTrades);
            _tradeHistory = CloneTradeHistory(snapshot.TradeHistory);
        }
    }

    private static string? ExtractOrderId(string text)
    {
        var match = Regex.Match(text ?? string.Empty, @"Order ID:\s*(\S+)", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value : null;
    }

    private static List<WalletTransactionEntry> CloneWalletHistory(IEnumerable<WalletTransactionEntry> source)
    {
        return source.Select(x => new WalletTransactionEntry
        {
            Timestamp = x.Timestamp,
            From = x.From,
            To = x.To,
            CryptoType = x.CryptoType,
            Amount = x.Amount,
            Status = x.Status
        }).ToList();
    }

    private static List<BotTradeRecord> CloneTradeHistory(IEnumerable<BotTradeRecord> source)
    {
        return source.Select(x => new BotTradeRecord
        {
            Timestamp = x.Timestamp,
            OrderId = x.OrderId,
            Exchange = x.Exchange,
            Side = x.Side,
            Pair = x.Pair,
            Amount = x.Amount,
            Price = x.Price,
            Status = x.Status,
            ExecutionPayload = x.ExecutionPayload
        }).ToList();
    }

    private static object MapTrade(BotTradeRecord trade)
    {
        return new
        {
            timestamp = trade.Timestamp.ToString("yyyy-MM-dd HH:mm:ss") + " UTC",
            orderId = trade.OrderId,
            exchange = trade.Exchange,
            side = trade.Side,
            pair = trade.Pair,
            amount = trade.Amount,
            price = trade.Price,
            status = trade.Status
        };
    }

    private static object MapWallet(WalletTransactionEntry item)
    {
        return new
        {
            timestamp = item.Timestamp.ToString("yyyy-MM-dd HH:mm:ss") + " UTC",
            from = item.From,
            to = item.To,
            cryptoType = item.CryptoType,
            amount = item.Amount,
            status = item.Status
        };
    }
}
