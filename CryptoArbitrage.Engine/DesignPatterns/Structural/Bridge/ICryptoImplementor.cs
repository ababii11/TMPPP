namespace CryptoArbitrage.Engine.DesignPatterns.Structural.Bridge;

/// <summary>
/// Implementor Interface - Defines low-level operations for crypto systems.
/// This interface bridges the gap between high-level crypto operations and 
/// specific implementation details (blockchain, APIs, etc.).
/// </summary>
public interface ICryptoImplementor
{
    /// <summary>
    /// Processes a transaction at the implementation level.
    /// </summary>
    /// <param name="transactionData">Raw transaction data</param>
    /// <returns>Transaction ID or confirmation hash</returns>
    string ProcessTransaction(string transactionData);

    /// <summary>
    /// Encrypts sensitive data using the implementation's encryption method.
    /// </summary>
    /// <param name="plainData">Data to encrypt</param>
    /// <returns>Encrypted data</returns>
    string EncryptData(string plainData);

    /// <summary>
    /// Validates a block or transaction according to the implementation's rules.
    /// </summary>
    /// <param name="blockData">Block or transaction to validate</param>
    /// <returns>Validation result</returns>
    bool ValidateBlock(string blockData);

    /// <summary>
    /// Gets the name/type of the implementation.
    /// </summary>
    string GetImplementationName();
}
