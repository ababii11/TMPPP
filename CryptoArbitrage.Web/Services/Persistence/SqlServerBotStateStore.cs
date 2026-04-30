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

        var snapshots = await LoadSnapshotsAsync(cancellationToken);
        if (snapshots.Count > 0)
        {
            var latest = snapshots[0];
            originator.RestoreFromMemento(latest);
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

        await using var command = connection.CreateCommand();
        command.CommandText = """
            MERGE bot_snapshots AS target
            USING (SELECT @SnapshotId AS snapshot_id) AS source
            ON target.snapshot_id = source.snapshot_id
            WHEN MATCHED THEN
                UPDATE SET
                    captured_at = @CapturedAt,
                    label = @Label,
                    balance = @Balance,
                    balance_currency = @BalanceCurrency,
                    wallet_name = @WalletName,
                    active_strategy = @ActiveStrategy,
                    wallet_history_json = @WalletHistoryJson,
                    open_trades_json = @OpenTradesJson,
                    trade_history_json = @TradeHistoryJson
            WHEN NOT MATCHED THEN
                INSERT (
                    snapshot_id,
                    captured_at,
                    label,
                    balance,
                    balance_currency,
                    wallet_name,
                    active_strategy,
                    wallet_history_json,
                    open_trades_json,
                    trade_history_json
                )
                VALUES (
                    @SnapshotId,
                    @CapturedAt,
                    @Label,
                    @Balance,
                    @BalanceCurrency,
                    @WalletName,
                    @ActiveStrategy,
                    @WalletHistoryJson,
                    @OpenTradesJson,
                    @TradeHistoryJson
                );
            """;

        command.Parameters.AddWithValue("@SnapshotId", snapshot.SnapshotId);
        command.Parameters.AddWithValue("@CapturedAt", snapshot.CapturedAt);
        command.Parameters.AddWithValue("@Label", snapshot.Label);
        command.Parameters.AddWithValue("@Balance", snapshot.Balance);
        command.Parameters.AddWithValue("@BalanceCurrency", snapshot.BalanceCurrency);
        command.Parameters.AddWithValue("@WalletName", snapshot.WalletName);
        command.Parameters.AddWithValue("@ActiveStrategy", snapshot.ActiveStrategy);
        command.Parameters.AddWithValue("@WalletHistoryJson", JsonSerializer.Serialize(snapshot.WalletHistory, _jsonOptions));
        command.Parameters.AddWithValue("@OpenTradesJson", JsonSerializer.Serialize(snapshot.OpenTrades, _jsonOptions));
        command.Parameters.AddWithValue("@TradeHistoryJson", JsonSerializer.Serialize(snapshot.TradeHistory, _jsonOptions));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BotStateSnapshot>> LoadSnapshotsAsync(CancellationToken cancellationToken = default)
    {
        await EnsureDatabaseAndSchemaAsync(cancellationToken);

        var snapshots = new List<BotStateSnapshot>();

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT snapshot_id, captured_at, label, balance, balance_currency, wallet_name, active_strategy,
                   wallet_history_json, open_trades_json, trade_history_json
            FROM bot_snapshots
            ORDER BY captured_at DESC;
            """;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            snapshots.Add(MapSnapshot(reader));
        }

        return snapshots;
    }

    private async Task EnsureDatabaseAndSchemaAsync(CancellationToken cancellationToken)
    {
        await EnsureDatabaseExistsAsync(cancellationToken);

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            IF OBJECT_ID('dbo.bot_snapshots', 'U') IS NULL
            BEGIN
                CREATE TABLE dbo.bot_snapshots (
                    snapshot_id NVARCHAR(64) NOT NULL PRIMARY KEY,
                    captured_at DATETIME2 NOT NULL,
                    label NVARCHAR(200) NOT NULL,
                    balance DECIMAL(18, 8) NOT NULL,
                    balance_currency NVARCHAR(20) NOT NULL,
                    wallet_name NVARCHAR(100) NOT NULL,
                    active_strategy NVARCHAR(120) NOT NULL,
                    wallet_history_json NVARCHAR(MAX) NOT NULL,
                    open_trades_json NVARCHAR(MAX) NOT NULL,
                    trade_history_json NVARCHAR(MAX) NOT NULL
                );
            END
            """;

        await command.ExecuteNonQueryAsync(cancellationToken);
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

    private BotStateSnapshot MapSnapshot(SqlDataReader reader)
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
            WalletHistory = DeserializeWalletHistory(reader.GetString(7)),
            OpenTrades = DeserializeTradeHistory(reader.GetString(8)),
            TradeHistory = DeserializeTradeHistory(reader.GetString(9))
        };
    }

    private IReadOnlyList<WalletTransactionEntry> DeserializeWalletHistory(string json)
    {
        return JsonSerializer.Deserialize<List<WalletTransactionEntry>>(json, _jsonOptions) ?? new List<WalletTransactionEntry>();
    }

    private IReadOnlyList<BotTradeRecord> DeserializeTradeHistory(string json)
    {
        return JsonSerializer.Deserialize<List<BotTradeRecord>>(json, _jsonOptions) ?? new List<BotTradeRecord>();
    }
}