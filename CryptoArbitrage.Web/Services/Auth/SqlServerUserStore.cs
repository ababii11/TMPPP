using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;

namespace CryptoArbitrage.Web.Services.Auth;

public sealed class SqlServerUserStore : IUserStore
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100_000;

    private readonly string _connectionString;

    public SqlServerUserStore(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("BotState")
            ?? throw new InvalidOperationException("Missing connection string: BotState");
    }

    public async Task EnsureSchemaAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'app_users')
BEGIN
    CREATE TABLE dbo.app_users (
        user_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        username NVARCHAR(64) NOT NULL UNIQUE,
        password_hash NVARCHAR(128) NOT NULL,
        password_salt NVARCHAR(64) NOT NULL,
        created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        last_login_at DATETIME2 NULL
    );
END
";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<UserRecord?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT user_id, username
FROM dbo.app_users
WHERE username = @username;
";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@username", username);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new UserRecord(reader.GetInt32(0), reader.GetString(1));
    }

    public async Task<UserRecord> CreateUserAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var (hash, salt) = HashPassword(password);

        const string sql = @"
INSERT INTO dbo.app_users (username, password_hash, password_salt)
OUTPUT INSERTED.user_id, INSERTED.username
VALUES (@username, @hash, @salt);
";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@username", username);
        command.Parameters.AddWithValue("@hash", hash);
        command.Parameters.AddWithValue("@salt", salt);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException("Failed to create user.");
        }

        return new UserRecord(reader.GetInt32(0), reader.GetString(1));
    }

    public async Task<bool> ValidateUserAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT password_hash, password_salt
FROM dbo.app_users
WHERE username = @username;
";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@username", username);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return false;
        }

        var storedHash = reader.GetString(0);
        var storedSalt = reader.GetString(1);

        return VerifyPassword(password, storedHash, storedSalt);
    }

    public async Task UpdateLastLoginAsync(int userId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
UPDATE dbo.app_users
SET last_login_at = SYSUTCDATETIME()
WHERE user_id = @userId;
";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@userId", userId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static (string Hash, string Salt) HashPassword(string password)
    {
        var saltBytes = RandomNumberGenerator.GetBytes(SaltSize);
        var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            saltBytes,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        return (Convert.ToBase64String(hashBytes), Convert.ToBase64String(saltBytes));
    }

    private static bool VerifyPassword(string password, string storedHash, string storedSalt)
    {
        var saltBytes = Convert.FromBase64String(storedSalt);
        var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            saltBytes,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        var candidate = Convert.ToBase64String(hashBytes);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(candidate),
            Encoding.UTF8.GetBytes(storedHash));
    }
}
