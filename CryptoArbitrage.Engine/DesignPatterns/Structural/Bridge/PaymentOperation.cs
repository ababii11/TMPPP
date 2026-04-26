namespace CryptoArbitrage.Engine.DesignPatterns.Structural.Bridge;

/// <summary>
/// Refined Abstraction #2 - PaymentOperation
/// Represents a crypto payment operation (e.g., paying for goods/services with crypto).
/// Delegates low-level details to the implementor.
/// </summary>
public class PaymentOperation : CryptoOperation
{
    private readonly string _merchantId;
    private readonly decimal _paymentAmount;
    private readonly string _currency;
    private readonly string _orderId;

    public PaymentOperation(
        ICryptoImplementor implementor,
        string merchantId,
        decimal paymentAmount,
        string currency = "ETH",
        string orderId = "")
        : base(implementor)
    {
        _merchantId = merchantId;
        _paymentAmount = paymentAmount;
        _currency = currency;
        _orderId = orderId ?? Guid.NewGuid().ToString();
    }

    public override void Execute()
    {
        Console.WriteLine($"\n=== PaymentOperation Started ===");
        Console.WriteLine($"Merchant: {_merchantId}, Amount: {_paymentAmount} {_currency}, Order ID: {_orderId}");
        Console.WriteLine($"Using Implementation: {Implementor.GetImplementationName()}\n");

        // Step 1: Validate payment data
        string paymentData = $"ORDER:{_orderId}|MERCHANT:{_merchantId}|AMOUNT:{_paymentAmount}{_currency}";
        bool isValid = Implementor.ValidateBlock(paymentData);

        if (!isValid)
        {
            Console.WriteLine("  [PaymentOperation] ❌ Payment validation failed!");
            return;
        }

        // Step 2: Encrypt merchant information and order details
        string encryptedData = Implementor.EncryptData($"{_merchantId}:{_orderId}");

        // Step 3: Process the payment transaction
        string paymentId = Implementor.ProcessTransaction(encryptedData);

        Console.WriteLine($"\n  [PaymentOperation] ✓ Payment processed with ID: {paymentId}");
        Console.WriteLine($"  [PaymentOperation] Payment of {_paymentAmount} {_currency} to {_merchantId} confirmed!");
        Console.WriteLine($"=== PaymentOperation Finished ===\n");
    }

    public override string GetOperationInfo()
    {
        return $"{base.GetOperationInfo()} | Payment: {_paymentAmount} {_currency} to {_merchantId}";
    }
}
