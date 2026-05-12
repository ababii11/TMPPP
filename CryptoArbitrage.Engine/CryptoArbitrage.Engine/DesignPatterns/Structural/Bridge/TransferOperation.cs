namespace CryptoArbitrage.Engine.DesignPatterns.Structural.Bridge;

/// <summary>
/// Refined Abstraction #1 - TransferOperation
/// Represents a crypto transfer operation (e.g., sending coins from one wallet to another).
/// Delegates low-level details to the implementor.
/// </summary>
public class TransferOperation : CryptoOperation
{
    private readonly string _fromWallet;
    private readonly string _toWallet;
    private readonly decimal _amount;
    private readonly string _cryptocurrency;

    public TransferOperation(
        ICryptoImplementor implementor,
        string fromWallet,
        string toWallet,
        decimal amount,
        string cryptocurrency = "BTC")
        : base(implementor)
    {
        _fromWallet = fromWallet;
        _toWallet = toWallet;
        _amount = amount;
        _cryptocurrency = cryptocurrency;
    }

    public override void Execute()
    {
        Console.WriteLine($"\n=== TransferOperation Started ===");
        Console.WriteLine($"From: {_fromWallet}, To: {_toWallet}, Amount: {_amount} {_cryptocurrency}");
        Console.WriteLine($"Using Implementation: {Implementor.GetImplementationName()}\n");

        // Step 1: Validate the transfer data
        string transferData = $"{_fromWallet}→{_toWallet}:{_amount}{_cryptocurrency}";
        bool isValid = Implementor.ValidateBlock(transferData);

        if (!isValid)
        {
            Console.WriteLine("  [TransferOperation] ❌ Transfer validation failed!");
            return;
        }

        // Step 2: Encrypt sensitive wallet information
        string encryptedData = Implementor.EncryptData($"{_fromWallet}:{_toWallet}");

        // Step 3: Process the transaction
        string transactionId = Implementor.ProcessTransaction(encryptedData);

        Console.WriteLine($"\n  [TransferOperation] ✓ Transfer completed with ID: {transactionId}");
        Console.WriteLine($"=== TransferOperation Finished ===\n");
    }

    public override string GetOperationInfo()
    {
        return $"{base.GetOperationInfo()} | Transfer: {_amount} {_cryptocurrency} from {_fromWallet} to {_toWallet}";
    }
}
