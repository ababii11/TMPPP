namespace CryptoArbitrage.Web.Services.Auth;

public interface IUserStore
{
    Task EnsureSchemaAsync(CancellationToken cancellationToken = default);
    Task<UserRecord?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<UserRecord> CreateUserAsync(string username, string password, CancellationToken cancellationToken = default);
    Task<bool> ValidateUserAsync(string username, string password, CancellationToken cancellationToken = default);
    Task UpdateLastLoginAsync(int userId, CancellationToken cancellationToken = default);
}

public sealed record UserRecord(int UserId, string Username);
