namespace CryptoArbitrage.Engine.DesignPatterns.Structural.Bridge;

/// <summary>
/// Bridge Design Pattern - Demonstration
/// 
/// The Bridge pattern is used to decouple an abstraction from its implementation
/// so the two can vary independently.
/// 
/// In this example:
/// - Abstractions: CryptoOperation (abstract), TransferOperation, PaymentOperation
/// - Implementations: BlockchainNetwork, CryptoAPIService
/// 
/// Benefits:
/// 1. You can mix and match abstractions with different implementations
/// 2. Adding new abstractions or implementations doesn't affect existing code
/// 3. Each abstraction can switch implementations at runtime
/// </summary>
public class BridgeDemo
{
    public static void RunDemo()
    {
        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║         BRIDGE DESIGN PATTERN - CRYPTO DEMO                ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");

        // Create implementors (concrete implementations)
        ICryptoImplementor ethereumBlockchain = new BlockchainNetwork("Ethereum");
        ICryptoImplementor bitcoinBlockchain = new BlockchainNetwork("Bitcoin");
        ICryptoImplementor binanceAPI = new CryptoAPIService("Binance", "api-key-12345");
        ICryptoImplementor coinGeckoAPI = new CryptoAPIService("CoinGecko", "free-tier");

        Console.WriteLine("═" + new string('═', 62) + "═");
        Console.WriteLine("  SCENARIO 1: TransferOperation with BlockchainNetwork");
        Console.WriteLine("═" + new string('═', 62) + "═");

        // Create abstraction with Ethereum
        CryptoOperation transfer1 = new TransferOperation(
            ethereumBlockchain,
            "wallet_alice_0x1234",
            "wallet_bob_0x5678",
            2.5m,
            "ETH"
        );

        transfer1.Execute();
        Console.WriteLine($"Info: {transfer1.GetOperationInfo()}\n");

        // Switch to Bitcoin without creating a new object
        Console.WriteLine("═" + new string('═', 62) + "═");
        Console.WriteLine("  SCENARIO 2: Same TransferOperation, Different Implementation (Bitcoin)");
        Console.WriteLine("═" + new string('═', 62) + "═");

        transfer1.SwitchImplementor(bitcoinBlockchain);
        transfer1.Execute();
        Console.WriteLine($"Info: {transfer1.GetOperationInfo()}\n");

        // Create new TransferOperation with API
        Console.WriteLine("═" + new string('═', 62) + "═");
        Console.WriteLine("  SCENARIO 3: TransferOperation with CryptoAPIService");
        Console.WriteLine("═" + new string('═', 62) + "═");

        CryptoOperation transfer2 = new TransferOperation(
            binanceAPI,
            "user_charlie_123",
            "user_diana_456",
            0.5m,
            "BNB"
        );

        transfer2.Execute();
        Console.WriteLine($"Info: {transfer2.GetOperationInfo()}\n");

        // PaymentOperation with Blockchain
        Console.WriteLine("═" + new string('═', 62) + "═");
        Console.WriteLine("  SCENARIO 4: PaymentOperation with BlockchainNetwork");
        Console.WriteLine("═" + new string('═', 62) + "═");

        CryptoOperation payment1 = new PaymentOperation(
            ethereumBlockchain,
            "MERCHANT_AMAZON_001",
            10.75m,
            "ETH",
            "ORD-2026-001"
        );

        payment1.Execute();
        Console.WriteLine($"Info: {payment1.GetOperationInfo()}\n");

        // PaymentOperation with API
        Console.WriteLine("═" + new string('═', 62) + "═");
        Console.WriteLine("  SCENARIO 5: PaymentOperation with CryptoAPIService");
        Console.WriteLine("═" + new string('═', 62) + "═");

        CryptoOperation payment2 = new PaymentOperation(
            coinGeckoAPI,
            "MERCHANT_STARBUCKS_007",
            5.0m,
            "BTC",
            "ORD-2026-002"
        );

        payment2.Execute();
        Console.WriteLine($"Info: {payment2.GetOperationInfo()}\n");

        // Demonstrate that implementations can be swapped for existing operations
        Console.WriteLine("═" + new string('═', 62) + "═");
        Console.WriteLine("  SCENARIO 6: PaymentOperation Switching Implementations");
        Console.WriteLine("═" + new string('═', 62) + "═");

        Console.WriteLine("Initial implementation: CoinGecko API (working)");
        Console.WriteLine("Switching to Binance API...\n");

        payment2.SwitchImplementor(binanceAPI);
        payment2.Execute();
        Console.WriteLine($"Info: {payment2.GetOperationInfo()}\n");

        // Summary
        PrintSummary();
    }

    private static void PrintSummary()
    {
        Console.WriteLine("═" + new string('═', 62) + "═");
        Console.WriteLine("  BRIDGE PATTERN BENEFITS DEMONSTRATED");
        Console.WriteLine("═" + new string('═', 62) + "═\n");

        Console.WriteLine("✓ Abstraction & Implementation Decoupling:");
        Console.WriteLine("  - CryptoOperation (abstraction) is independent of ICryptoImplementor (implementation)");
        Console.WriteLine("  - Changes to BlockchainNetwork don't affect TransferOperation or PaymentOperation\n");

        Console.WriteLine("✓ Runtime Implementation Switching:");
        Console.WriteLine("  - TransferOperation switched from Ethereum → Bitcoin seamlessly");
        Console.WriteLine("  - PaymentOperation switched from CoinGecko → Binance without object recreation\n");

        Console.WriteLine("✓ Independent Scalability:");
        Console.WriteLine("  - Can add new Abstraction (e.g., StakingOperation) without touching existing code");
        Console.WriteLine("  - Can add new Implementor (e.g., LightningNetwork) without touching abstractions\n");

        Console.WriteLine("✓ Flexibility:");
        Console.WriteLine("  - Created 2 Abstractions × 4 Implementations = 8 different combinations");
        Console.WriteLine("  - Each combination works independently through the bridge\n");

        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║              DEMO COMPLETED SUCCESSFULLY                   ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");
    }
}
