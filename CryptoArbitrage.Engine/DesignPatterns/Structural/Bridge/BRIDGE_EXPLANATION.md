# Bridge Design Pattern - Crypto Implementation

## Overview

The **Bridge Pattern** decouples an abstraction from its implementation so that the two can vary independently. This prevents a "fragile base class" problem where changes in one dimension (implementation) force changes in another (abstraction).

## Structure

```
┌─────────────────────────────────────────────────────────────────┐
│                            Client                               │
└──────────────────────┬──────────────────────────────────────────┘
                       │
                       │ uses
                       ▼
┌──────────────────────────────────────┐  ┌──────────────────────┐
│    CryptoOperation (Abstraction)     │  │ Refined Abstractions │
│  ─────────────────────────────────   │  │ ──────────────────── │
│  # implementor: ICryptoImplementor   │  │ • TransferOperation  │
│  + Execute(): void                   │  │ • PaymentOperation   │
│  + SwitchImplementor(): void         │  │ • (Future ops)       │
└──────────────────────────────────────┘  └──────────────────────┘
         │ depends on
         │ (THIS IS THE BRIDGE)
         ▼
┌──────────────────────────────────────┐  ┌──────────────────────┐
│ ICryptoImplementor (Implementor)     │  │ Concrete Implementors│
│ ──────────────────────────────────── │  │ ──────────────────── │
│ + ProcessTransaction(): string       │  │ • BlockchainNetwork  │
│ + EncryptData(): string              │  │ • CryptoAPIService   │
│ + ValidateBlock(): bool              │  │ • (Future impls)     │
│ + GetImplementationName(): string    │  │                      │
└──────────────────────────────────────┘  └──────────────────────┘
```

## Components

### 1. **Implementor Interface** (`ICryptoImplementor`)
Defines the contract for low-level crypto operations:
- `ProcessTransaction()` - Handles transaction at implementation level
- `EncryptData()` - Encrypts sensitive data
- `ValidateBlock()` - Validates blocks/transactions
- `GetImplementationName()` - Returns implementation identifier

### 2. **Concrete Implementors**

#### **BlockchainNetwork**
- Simulates direct blockchain interaction (Ethereum, Bitcoin, etc.)
- Uses blockchain-specific encryption (Keccak-256)
- Implements blockchain validation rules
- Network-level processing with simulated delays

#### **CryptoAPIService**
- Simulates external API calls (Binance, CoinGecko, etc.)
- Uses API-standard encryption (AES-256 Base64)
- Implements API schema validation
- API-specific response handling

### 3. **Abstraction Class** (`CryptoOperation`)
High-level interface that:
- Holds a reference to `ICryptoImplementor` (the bridge)
- Defines abstract `Execute()` method
- Allows runtime switching of implementations via `SwitchImplementor()`

### 4. **Refined Abstractions**

#### **TransferOperation**
- Transfers crypto between wallets
- Validates transfer data → Encrypts wallet info → Processes transaction
- Can use any implementor (blockchain or API)

#### **PaymentOperation**
- Handles payment transactions
- Validates payment data → Encrypts merchant info → Processes payment
- Can use any implementor (blockchain or API)

## Key Benefits

### ✅ **Decoupling**
- Abstractions (TransferOperation, PaymentOperation) are completely independent of implementations
- Changing how BlockchainNetwork works doesn't affect existing abstractions

### ✅ **Scalability**
- Add new abstraction (e.g., `StakingOperation`) without modifying existing code
- Add new implementor (e.g., `LightningNetwork`) without modifying abstractions
- Number of combinations grows independently: N abstractions × M implementors

### ✅ **Runtime Flexibility**
- Switch implementations at runtime:
  ```csharp
  transfer1.SwitchImplementor(bitcoinBlockchain);
  transfer1.Execute(); // Now uses Bitcoin instead of Ethereum
  ```

### ✅ **Avoids Fragile Base Class Problem**
Without Bridge: `TransferOperation_Blockchain_Ethereum`, `TransferOperation_API_Binance`, etc.
With Bridge: `TransferOperation` + implementations (2+4 simple classes vs. exponential growth)

## Usage Examples

### Example 1: Transfer with Different Implementations
```csharp
// Same abstraction, different implementations
CryptoOperation transfer = new TransferOperation(
    new BlockchainNetwork("Ethereum"),
    wallet1, wallet2, amount, "ETH"
);
transfer.Execute();

// Switch implementation at runtime
transfer.SwitchImplementor(new CryptoAPIService("Binance"));
transfer.Execute(); // Now uses API instead of blockchain
```

### Example 2: Create Multiple Combinations
```csharp
var operations = new List<CryptoOperation>
{
    new TransferOperation(ethereumBlockchain, ...),    // Abstraction A, Impl 1
    new TransferOperation(binanceAPI, ...),            // Abstraction A, Impl 2
    new PaymentOperation(ethereumBlockchain, ...),     // Abstraction B, Impl 1
    new PaymentOperation(coinGeckoAPI, ...)            // Abstraction B, Impl 2
};

foreach (var op in operations)
    op.Execute(); // All work seamlessly
```

## Design Pattern Principles

| Principle | How Bridge Implements It |
|-----------|------------------------|
| **Single Responsibility** | CryptoOperation handles high-level logic; BlockchainNetwork handles blockchain specifics |
| **Open/Closed** | Open for extension (add new operations/implementors), closed for modification |
| **Liskov Substitution** | Any ICryptoImplementor can replace another without breaking abstractions |
| **Interface Segregation** | ICryptoImplementor defines minimal required methods |
| **Dependency Inversion** | High-level abstractions depend on ICryptoImplementor interface, not concrete classes |

## Comparison with Other Patterns

| Pattern | Purpose | When to Use |
|---------|---------|------------|
| **Bridge** | Decouple abstraction from implementation | When both abstract and concrete dimensions change independently |
| **Adapter** | Make incompatible interfaces work together | When integrating existing classes |
| **Facade** | Simplify complex subsystems | When hiding complexity from clients |
| **Decorator** | Add behavior dynamically | When adding responsibilities to individual objects |

## Real-World Applications in Crypto

1. **Multi-Chain Support**: Use different blockchains (Ethereum, Bitcoin, Solana) without changing transaction logic
2. **API Flexibility**: Switch between exchange APIs without rewriting order logic
3. **Testing**: Replace real blockchain/API with mock implementations for unit tests
4. **Legacy Integration**: Bridge between old crypto implementations and new abstraction layers
5. **Runtime Configuration**: Select optimal implementation based on network conditions or cost

## Files in This Implementation

- `ICryptoImplementor.cs` - Implementor interface (contract)
- `BlockchainNetwork.cs` - Concrete implementor for blockchain
- `CryptoAPIService.cs` - Concrete implementor for APIs
- `CryptoOperation.cs` - Abstract base class (Abstraction)
- `TransferOperation.cs` - Refined abstraction for transfers
- `PaymentOperation.cs` - Refined abstraction for payments
- `BridgeDemo.cs` - Demonstration of all combinations
- `bridgediagram.puml` - UML diagram (PlantUML format)
- `BRIDGE_EXPLANATION.md` - This file

## Running the Demo

To run the demonstration:

```csharp
BridgeDemo.RunDemo();
```

Expected output shows:
1. ✓ Transfer operations using Ethereum blockchain
2. ✓ Same transfer switching to Bitcoin blockchain
3. ✓ Transfer using Binance API
4. ✓ Payment using Ethereum blockchain
5. ✓ Payment using CoinGecko API
6. ✓ Payment switching from CoinGecko to Binance

This demonstrates that both abstractions and implementations vary independently!
