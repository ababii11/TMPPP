namespace CryptoArbitrage.Engine.DesignPatterns.Structural.Proxy;

/// <summary>
/// ICryptoService Interface - Defines crypto operations that require access control in a proxy pattern.
/// This interface establishes the contract for secure cryptocurrency transactions and wallet operations.
/// </summary>
public interface ICryptoService
{
    /// <summary>
    /// Executes a cryptocurrency transaction from one wallet to another.
    /// </summary>
    /// <param name="fromWallet">Source wallet address</param>
    /// <param name="toWallet">Destination wallet address</param>
    /// <param name="amount">Transaction amount in cryptocurrency</param>
    /// <param name="cryptocurrency">Type of cryptocurrency (e.g., BTC, ETH, USDT)</param>
    /// <returns>Transaction hash/ID if successful</returns>
    string ExecuteTransaction(string fromWallet, string toWallet, decimal amount, string cryptocurrency);

    /// <summary>
    /// Retrieves the balance of a wallet.
    /// </summary>
    /// <param name="walletAddress">The wallet address to query</param>
    /// <param name="cryptocurrency">Type of cryptocurrency</param>
    /// <returns>Wallet balance</returns>
    decimal GetWalletBalance(string walletAddress, string cryptocurrency);

    /// <summary>
    /// Gets the current market price of a cryptocurrency.
    /// </summary>
    /// <param name="cryptocurrency">Type of cryptocurrency</param>
    /// <returns>Current price in USD</returns>
    decimal GetCryptoPrice(string cryptocurrency);
}
