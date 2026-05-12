using System.Globalization;
using System.Text.Json;
using CryptoArbitrage.Engine.DesignPatterns.Behavioral.Memento;
using Microsoft.Data.SqlClient;

namespace CryptoArbitrage.Web.Services.Persistence;

public interface IBotStateStore
{
    Task InitializeAsync(ArbitrageBotStateOriginator originator, StateManager stateManager, CancellationToken cancellationToken = default);
    Task PersistSnapshotAsync(BotStateSnapshot snapshot, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BotStateSnapshot>> LoadSnapshotsAsync(CancellationToken cancellationToken = default);
}

public sealed class SqlServerBotStateStore : IBotStateStore
{
    private readonly string _connectionString;
    private readonly string _serverConnectionString;
    private readonly string _databaseName;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public SqlServerBotStateStore(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("BotState")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:BotState configuration.");

        var builder = new SqlConnectionStringBuilder(connectionString);
        _databaseName = string.IsNullOrWhiteSpace(builder.InitialCatalog) ? "CryptoArbitrageBot" : builder.InitialCatalog;
        builder.InitialCatalog = _databaseName;
        _connectionString = builder.ConnectionString;

        var serverBuilder = new SqlConnectionStringBuilder(_connectionString)
        {
            InitialCatalog = "master"
        };
        _serverConnectionString = serverBuilder.ConnectionString;
    }

    public async Task InitializeAsync(ArbitrageBotStateOriginator originator, StateManager stateManager, CancellationToken cancellationToken = default)
    {
        await EnsureDatabaseAndSchemaAsync(cancellationToken);
        await MigrateLegacySnapshotsIfNeededAsync(cancellationToken);

        var snapshots = await LoadSnapshotsAsync(cancellationToken);
        if (snapshots.Count > 0)
        {
            originator.RestoreFromMemento(snapshots[0]);
            stateManager.LoadSnapshots(snapshots);
            return;
        }

        var seedSnapshot = originator.SaveToMemento("initial-seed");
        stateManager.LoadSnapshots(new[] { seedSnapshot });
        await PersistSnapshotAsync(seedSnapshot, cancellationToken);
    }

    public async Task PersistSnapshotAsync(BotStateSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        await EnsureDatabaseAndSchemaAsync(cancellationToken);

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await UpsertSnapshotHeaderAsync(connection, transaction, snapshot, cancellationToken);
            await ReplaceWalletTransactionsAsync(connection, transaction, snapshot, cancellationToken);
            await ReplaceTradeExecutionsAsync(connection, transaction, snapshot, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<IReadOnlyList<BotStateSnapshot>> LoadSnapshotsAsync(CancellationToken cancellationToken = default)
    {
        await EnsureDatabaseAndSchemaAsync(cancellationToken);

        var snapshots = new List<BotStateSnapshot>();

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT snapshot_id, captured_at, label, balance, balance_currency, wallet_name, active_strategy
FROM dbo.bot_state_snapshots
ORDER BY captured_at DESC;";

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            snapshots.Add(await MapSnapshotAsync(connection, reader, cancellationToken));
        }

        return snapshots;
    }

    private async Task EnsureDatabaseAndSchemaAsync(CancellationToken cancellationToken)
    {
        await EnsureDatabaseExistsAsync(cancellationToken);

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"
IF OBJECT_ID('dbo.bot_state_snapshots', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.bot_state_snapshots (
        snapshot_id NVARCHAR(64) NOT NULL PRIMARY KEY,
        captured_at DATETIME2 NOT NULL,
        label NVARCHAR(200) NOT NULL,
        balance DECIMAL(18, 8) NOT NULL,
        balance_currency NVARCHAR(20) NOT NULL,
        wallet_name NVARCHAR(100) NOT NULL,
        active_strategy NVARCHAR(120) NOT NULL
    );
END;

IF OBJECT_ID('dbo.bot_wallet_transactions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.bot_wallet_transactions (
        wallet_transaction_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        snapshot_id NVARCHAR(64) NOT NULL,
        transaction_timestamp DATETIME2 NOT NULL,
        source_address NVARCHAR(120) NOT NULL,
        destination_address NVARCHAR(120) NOT NULL,
        crypto_type NVARCHAR(20) NOT NULL,
        amount DECIMAL(18, 8) NOT NULL,
        status NVARCHAR(40) NOT NULL,
        CONSTRAINT FK_bot_wallet_transactions_snapshot
            FOREIGN KEY (snapshot_id) REFERENCES dbo.bot_state_snapshots(snapshot_id) ON DELETE CASCADE
    );
    CREATE INDEX IX_bot_wallet_transactions_snapshot_id ON dbo.bot_wallet_transactions(snapshot_id);
END;

IF OBJECT_ID('dbo.bot_trade_executions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.bot_trade_executions (
        trade_execution_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        snapshot_id NVARCHAR(64) NOT NULL,
        trade_timestamp DATETIME2 NOT NULL,
        trade_bucket NVARCHAR(20) NOT NULL,
        order_id NVARCHAR(100) NOT NULL,
        exchange_name NVARCHAR(40) NOT NULL,
        side NVARCHAR(12) NOT NULL,
        pair NVARCHAR(40) NOT NULL,
        amount DECIMAL(18, 8) NOT NULL,
        price DECIMAL(18, 8) NOT NULL,
        status NVARCHAR(40) NOT NULL,
        execution_payload NVARCHAR(MAX) NOT NULL,
        CONSTRAINT FK_bot_trade_executions_snapshot
            FOREIGN KEY (snapshot_id) REFERENCES dbo.bot_state_snapshots(snapshot_id) ON DELETE CASCADE
    );
    CREATE INDEX IX_bot_trade_executions_snapshot_id ON dbo.bot_trade_executions(snapshot_id);
END;";

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task MigrateLegacySnapshotsIfNeededAsync(CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        if (!await TableExistsAsync(connection, "dbo.bot_snapshots", cancellationToken))
        {
            return;
        }

        if (!await TableExistsAsync(connection, "dbo.bot_state_snapshots", cancellationToken))
        {
            return;
        }

        await using var countCommand = connection.CreateCommand();
        countCommand.CommandText = "SELECT COUNT(1) FROM dbo.bot_state_snapshots;";
        var normalizedCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
        if (normalizedCount > 0)
        {
            return;
        }

        await using var legacyCommand = connection.CreateCommand();
        legacyCommand.CommandText = @"
SELECT snapshot_id, captured_at, label, balance, balance_currency, wallet_name, active_strategy,
       wallet_history_json, open_trades_json, trade_history_json
FROM dbo.bot_snapshots
ORDER BY captured_at DESC;";

        await using var reader = await legacyCommand.ExecuteReaderAsync(cancellationToken);
        var legacySnapshots = new List<BotStateSnapshot>();
        while (await reader.ReadAsync(cancellationToken))
        {
            legacySnapshots.Add(MapLegacySnapshot(reader));
        }

        foreach (var snapshot in legacySnapshots)
        {
            await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);
            try
            {
                await UpsertSnapshotHeaderAsync(connection, transaction, snapshot, cancellationToken);
                await ReplaceWalletTransactionsAsync(connection, transaction, snapshot, cancellationToken);
                await ReplaceTradeExecutionsAsync(connection, transaction, snapshot, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }

    private static async Task<bool> TableExistsAsync(SqlConnection connection, string tableName, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT CASE WHEN OBJECT_ID(@TableName, 'U') IS NULL THEN 0 ELSE 1 END;";
        command.Parameters.AddWithValue("@TableName", tableName);
        var result = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
        return result == 1;
    }

    private BotStateSnapshot MapLegacySnapshot(SqlDataReader reader)
    {
        return new BotStateSnapshot
        {
            SnapshotId = reader.GetString(0),
            CapturedAt = reader.GetDateTime(1),
            Label = reader.GetString(2),
            Balance = reader.GetDecimal(3),
            BalanceCurrency = reader.GetString(4),
            WalletName = reader.GetString(5),
            ActiveStrategy = reader.GetString(6),
            WalletHistory = JsonSerializer.Deserialize<List<WalletTransactionEntry>>(reader.GetString(7), _jsonOptions) ?? new List<WalletTransactionEntry>(),
            OpenTrades = JsonSerializer.Deserialize<List<BotTradeRecord>>(reader.GetString(8), _jsonOptions) ?? new List<BotTradeRecord>(),
            TradeHistory = JsonSerializer.Deserialize<List<BotTradeRecord>>(reader.GetString(9), _jsonOptions) ?? new List<BotTradeRecord>()
        };
    }

    private async Task UpsertSnapshotHeaderAsync(SqlConnection connection, SqlTransaction transaction, BotStateSnapshot snapshot, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = @"
MERGE dbo.bot_state_snapshots AS target
USING (SELECT @SnapshotId AS snapshot_id) AS source
ON target.snapshot_id = source.snapshot_id
WHEN MATCHED THEN
    UPDATE SET
        captured_at = @CapturedAt,
        label = @Label,
        balance = @Balance,
        balance_currency = @BalanceCurrency,
        wallet_name = @WalletName,
        active_strategy = @ActiveStrategy
WHEN NOT MATCHED THEN
    INSERT (snapshot_id, captured_at, label, balance, balance_currency, wallet_name, active_strategy)
    VALUES (@SnapshotId, @CapturedAt, @Label, @Balance, @BalanceCurrency, @WalletName, @ActiveStrategy);";

        command.Parameters.AddWithValue("@SnapshotId", snapshot.SnapshotId);
        command.Parameters.AddWithValue("@CapturedAt", snapshot.CapturedAt);
        command.Parameters.AddWithValue("@Label", snapshot.Label);
        command.Parameters.AddWithValue("@Balance", snapshot.Balance);
        command.Parameters.AddWithValue("@BalanceCurrency", snapshot.BalanceCurrency);
        command.Parameters.AddWithValue("@WalletName", snapshot.WalletName);
        command.Parameters.AddWithValue("@ActiveStrategy", snapshot.ActiveStrategy);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task ReplaceWalletTransactionsAsync(SqlConnection connection, SqlTransaction transaction, BotStateSnapshot snapshot, CancellationToken cancellationToken)
    {
        await using (var deleteCommand = connection.CreateCommand())
        {
            deleteCommand.Transaction = transaction;
            deleteCommand.CommandText = "DELETE FROM dbo.bot_wallet_transactions WHERE snapshot_id = @SnapshotId;";
            deleteCommand.Parameters.AddWithValue("@SnapshotId", snapshot.SnapshotId);
            await deleteCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        foreach (var walletEntry in snapshot.WalletHistory)
        {
            await using var insertCommand = connection.CreateCommand();
            insertCommand.Transaction = transaction;
            insertCommand.CommandText = @"
INSERT INTO dbo.bot_wallet_transactions (
    snapshot_id,
    transaction_timestamp,
    source_address,
    destination_address,
    crypto_type,
    amount,
    status
) VALUES (
    @SnapshotId,
    @TransactionTimestamp,
    @SourceAddress,
    @DestinationAddress,
    @CryptoType,
    @Amount,
    @Status
);";

            insertCommand.Parameters.AddWithValue("@SnapshotId", snapshot.SnapshotId);
            insertCommand.Parameters.AddWithValue("@TransactionTimestamp", walletEntry.Timestamp);
            insertCommand.Parameters.AddWithValue("@SourceAddress", walletEntry.From);
            insertCommand.Parameters.AddWithValue("@DestinationAddress", walletEntry.To);
            insertCommand.Parameters.AddWithValue("@CryptoType", walletEntry.CryptoType);
            insertCommand.Parameters.AddWithValue("@Amount", walletEntry.Amount);
            insertCommand.Parameters.AddWithValue("@Status", walletEntry.Status);

            await insertCommand.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private async Task ReplaceTradeExecutionsAsync(SqlConnection connection, SqlTransaction transaction, BotStateSnapshot snapshot, CancellationToken cancellationToken)
    {
        await using (var deleteCommand = connection.CreateCommand())
        {
            deleteCommand.Transaction = transaction;
            deleteCommand.CommandText = "DELETE FROM dbo.bot_trade_executions WHERE snapshot_id = @SnapshotId;";
            deleteCommand.Parameters.AddWithValue("@SnapshotId", snapshot.SnapshotId);
            await deleteCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        foreach (var openTrade in snapshot.OpenTrades)
        {
            await InsertTradeAsync(connection, transaction, snapshot.SnapshotId, openTrade, "Open", cancellationToken);
        }

        foreach (var tradeHistory in snapshot.TradeHistory)
        {
            await InsertTradeAsync(connection, transaction, snapshot.SnapshotId, tradeHistory, "History", cancellationToken);
        }
    }

    private static async Task InsertTradeAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        string snapshotId,
        BotTradeRecord trade,
        string tradeBucket,
        CancellationToken cancellationToken)
    {
        await using var insertCommand = connection.CreateCommand();
        insertCommand.Transaction = transaction;
        insertCommand.CommandText = @"
INSERT INTO dbo.bot_trade_executions (
    snapshot_id,
    trade_timestamp,
    trade_bucket,
    order_id,
    exchange_name,
    side,
    pair,
    amount,
    price,
    status,
    execution_payload
) VALUES (
    @SnapshotId,
    @TradeTimestamp,
    @TradeBucket,
    @OrderId,
    @ExchangeName,
    @Side,
    @Pair,
    @Amount,
    @Price,
    @Status,
    @ExecutionPayload
);";

        insertCommand.Parameters.AddWithValue("@SnapshotId", snapshotId);
        insertCommand.Parameters.AddWithValue("@TradeTimestamp", trade.Timestamp);
        insertCommand.Parameters.AddWithValue("@TradeBucket", tradeBucket);
        insertCommand.Parameters.AddWithValue("@OrderId", trade.OrderId);
        insertCommand.Parameters.AddWithValue("@ExchangeName", trade.Exchange);
        insertCommand.Parameters.AddWithValue("@Side", trade.Side);
        insertCommand.Parameters.AddWithValue("@Pair", trade.Pair);
        insertCommand.Parameters.AddWithValue("@Amount", trade.Amount);
        insertCommand.Parameters.AddWithValue("@Price", trade.Price);
        insertCommand.Parameters.AddWithValue("@Status", trade.Status);
        insertCommand.Parameters.AddWithValue("@ExecutionPayload", trade.ExecutionPayload);

        await insertCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task<BotStateSnapshot> MapSnapshotAsync(SqlConnection connection, SqlDataReader reader, CancellationToken cancellationToken)
    {
        var snapshotId = reader.GetString(0);
        return new BotStateSnapshot
        {
            SnapshotId = snapshotId,
            CapturedAt = reader.GetDateTime(1),
            Label = reader.GetString(2),
            Balance = reader.GetDecimal(3),
            BalanceCurrency = reader.GetString(4),
            WalletName = reader.GetString(5),
            ActiveStrategy = reader.GetString(6),
            WalletHistory = await LoadWalletTransactionsAsync(connection, snapshotId, cancellationToken),
            OpenTrades = await LoadTradesAsync(connection, snapshotId, "Open", cancellationToken),
            TradeHistory = await LoadTradesAsync(connection, snapshotId, "History", cancellationToken)
        };
    }

    private async Task<IReadOnlyList<WalletTransactionEntry>> LoadWalletTransactionsAsync(SqlConnection connection, string snapshotId, CancellationToken cancellationToken)
    {
        var entries = new List<WalletTransactionEntry>();

        await using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT transaction_timestamp, source_address, destination_address, crypto_type, amount, status
FROM dbo.bot_wallet_transactions
WHERE snapshot_id = @SnapshotId
ORDER BY transaction_timestamp DESC, wallet_transaction_id DESC;";
        command.Parameters.AddWithValue("@SnapshotId", snapshotId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            entries.Add(new WalletTransactionEntry
            {
                Timestamp = reader.GetDateTime(0),
                From = reader.GetString(1),
                To = reader.GetString(2),
                CryptoType = reader.GetString(3),
                Amount = reader.GetDecimal(4),
                Status = reader.GetString(5)
            });
        }

        return entries;
    }

    private async Task<IReadOnlyList<BotTradeRecord>> LoadTradesAsync(SqlConnection connection, string snapshotId, string tradeBucket, CancellationToken cancellationToken)
    {
        var trades = new List<BotTradeRecord>();

        await using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT trade_timestamp, order_id, exchange_name, side, pair, amount, price, status, execution_payload
FROM dbo.bot_trade_executions
WHERE snapshot_id = @SnapshotId AND trade_bucket = @TradeBucket
ORDER BY trade_timestamp DESC, trade_execution_id DESC;";
        command.Parameters.AddWithValue("@SnapshotId", snapshotId);
        command.Parameters.AddWithValue("@TradeBucket", tradeBucket);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            trades.Add(new BotTradeRecord
            {
                Timestamp = reader.GetDateTime(0),
                OrderId = reader.GetString(1),
                Exchange = reader.GetString(2),
                Side = reader.GetString(3),
                Pair = reader.GetString(4),
                Amount = reader.GetDecimal(5),
                Price = reader.GetDecimal(6),
                Status = reader.GetString(7),
                ExecutionPayload = reader.GetString(8)
            });
        }

        return trades;
    }

    private async Task EnsureDatabaseExistsAsync(CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_serverConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var existsCommand = connection.CreateCommand();
        existsCommand.CommandText = "SELECT COUNT(1) FROM sys.databases WHERE name = @DatabaseName;";
        existsCommand.Parameters.AddWithValue("@DatabaseName", _databaseName);

        var exists = Convert.ToInt32(await existsCommand.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
        if (exists > 0)
        {
            return;
        }

        await using var createCommand = connection.CreateCommand();
        createCommand.CommandText = $"CREATE DATABASE [{_databaseName.Replace("]", "]]", StringComparison.Ordinal)};";
        await createCommand.ExecuteNonQueryAsync(cancellationToken);
    }
}