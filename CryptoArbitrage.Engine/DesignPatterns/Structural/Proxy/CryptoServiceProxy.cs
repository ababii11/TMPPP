namespace CryptoArbitrage.Engine.DesignPatterns.Structural.Proxy;

/// <summary>
/// CryptoServiceProxy - A protective proxy that controls access to RealCryptoService.
/// 
/// Responsibilities:
/// 1. Access Control: Validates API credentials before allowing operations
/// 2. Caching: Caches wallet balances and prices to reduce API calls
/// 3. Logging: Tracks all access attempts and operations
/// 4. Request Validation: Ensures parameters are valid before delegating
/// 
/// This proxy implements the Proxy Design Pattern exactly as per Refactoring Guru:
/// - Client → Interface → Proxy → Real Subject
/// - Proxy has same interface as RealService
/// - Proxy controls access to the real service
/// </summary>
public class CryptoServiceProxy : ICryptoService
{
    private readonly RealCryptoService _realService;
    private readonly string _apiKey;
    private readonly bool _isAuthenticated;
    private readonly Dictionary<string, (decimal balance, DateTime timestamp)> _balanceCache;
    private readonly Dictionary<string, (decimal price, DateTime timestamp)> _priceCache;
    private const int CACHE_DURATION_SECONDS = 60;

    public CryptoServiceProxy(RealCryptoService realService, string apiKey)
    {
        _realService = realService ?? throw new ArgumentNullException(nameof(realService));
        _apiKey = apiKey;
        _balanceCache = new Dictionary<string, (decimal, DateTime)>();
        _priceCache = new Dictionary<string, (decimal, DateTime)>();
        
        // Validate API key
        _isAuthenticated = ValidateApiKey(apiKey);
        
        if (!_isAuthenticated)
        {
            Console.WriteLine($"[CryptoServiceProxy] ⚠ AUTHENTICATION FAILED - Invalid API key provided");
        }
        else
        {
            Console.WriteLine($"[CryptoServiceProxy] ✓ AUTHENTICATED - API key validated");
        }
    }

    /// <summary>
    /// Validates API key format and credentials.
    /// Simulates authentication against a security system.
    /// </summary>
    private bool ValidateApiKey(string apiKey)
    {
        // Check if API key format is valid (e.g., starts with "api-" and is at least 8 chars)
        return !string.IsNullOrEmpty(apiKey) && 
               apiKey.StartsWith("api-") && 
               apiKey.Length >= 8;
    }

    /// <summary>
    /// Checks if user has access to perform operations.
    /// Also validates transaction parameters.
    /// </summary>
    private bool CheckAccess(string operation)
    {
        if (!_isAuthenticated)
        {
            Console.WriteLine($"[CryptoServiceProxy] ✗ ACCESS DENIED - {operation}");
            Console.WriteLine($"    Reason: User is not authenticated. Valid API key required.\n");
            return false;
        }

        Console.WriteLine($"[CryptoServiceProxy] ✓ ACCESS GRANTED - {operation}");
        return true;
    }

    /// <summary>
    /// Validates transaction parameters (amounts, addresses, etc.)
    /// </summary>
    private bool ValidateTransactionParameters(string fromWallet, string toWallet, decimal amount, string cryptocurrency)
    {
        // Check wallet addresses
        if (string.IsNullOrWhiteSpace(fromWallet) || string.IsNullOrWhiteSpace(toWallet))
        {
            Console.WriteLine($"[CryptoServiceProxy] ✗ VALIDATION FAILED - Invalid wallet address");
            return false;
        }

        // Check if wallets are different
        if (fromWallet.Equals(toWallet, StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"[CryptoServiceProxy] ✗ VALIDATION FAILED - Cannot transfer to the same wallet");
            return false;
        }

        // Check amount
        if (amount <= 0)
        {
            Console.WriteLine($"[CryptoServiceProxy] ✗ VALIDATION FAILED - Amount must be greater than zero");
            return false;
        }

        // Check maximum transaction limit (anti-fraud measure)
        if (amount > 1000000)
        {
            Console.WriteLine($"[CryptoServiceProxy] ✗ VALIDATION FAILED - Amount exceeds maximum transaction limit");
            return false;
        }

        // Check cryptocurrency
        if (string.IsNullOrWhiteSpace(cryptocurrency))
        {
            Console.WriteLine($"[CryptoServiceProxy] ✗ VALIDATION FAILED - Cryptocurrency type not specified");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if cached data is still valid.
    /// </summary>
    private bool IsCacheValid(DateTime timestamp)
    {
        return (DateTime.Now - timestamp).TotalSeconds < CACHE_DURATION_SECONDS;
    }

    /// <summary>
    /// Proxy implementation of ExecuteTransaction.
    /// 1. Checks authentication
    /// 2. Validates parameters
    /// 3. Logs the operation
    /// 4. Delegates to real service if approved
    /// </summary>
    public string ExecuteTransaction(string fromWallet, string toWallet, decimal amount, string cryptocurrency)
    {
        Console.WriteLine($"\n┌─ [CryptoServiceProxy] ExecuteTransaction Request");
        Console.WriteLine($"├─ From: {fromWallet} → To: {toWallet}");
        Console.WriteLine($"├─ Amount: {amount} {cryptocurrency}");

        // Step 1: Check access
        if (!CheckAccess("ExecuteTransaction"))
        {
            Console.WriteLine($"└─ [CryptoServiceProxy] Transaction BLOCKED\n");
            throw new UnauthorizedAccessException("API key validation failed. Transaction denied.");
        }

        // Step 2: Validate parameters
        if (!ValidateTransactionParameters(fromWallet, toWallet, amount, cryptocurrency))
        {
            Console.WriteLine($"└─ [CryptoServiceProxy] Transaction BLOCKED\n");
            throw new ArgumentException("Transaction parameters validation failed.");
        }

        // Step 3: Log and delegate
        Console.WriteLine($"├─ Status: All validations passed");
        Console.WriteLine($"└─ Delegating to RealCryptoService...\n");

        string result = _realService.ExecuteTransaction(fromWallet, toWallet, amount, cryptocurrency);
        
        Console.WriteLine($"[CryptoServiceProxy] Transaction completed successfully\n");
        return result;
    }

    /// <summary>
    /// Proxy implementation of GetWalletBalance.
    /// 1. Checks authentication
    /// 2. Checks cache first
    /// 3. Delegates to real service if cache miss
    /// 4. Updates cache
    /// </summary>
    public decimal GetWalletBalance(string walletAddress, string cryptocurrency)
    {
        Console.WriteLine($"\n┌─ [CryptoServiceProxy] GetWalletBalance Request");
        Console.WriteLine($"├─ Wallet: {walletAddress}");
        Console.WriteLine($"├─ Cryptocurrency: {cryptocurrency}");

        // Step 1: Check access
        if (!CheckAccess("GetWalletBalance"))
        {
            Console.WriteLine($"└─ [CryptoServiceProxy] Request BLOCKED\n");
            throw new UnauthorizedAccessException("API key validation failed.");
        }

        // Step 2: Check cache
        string cacheKey = $"{walletAddress}_{cryptocurrency}";
        if (_balanceCache.ContainsKey(cacheKey) && IsCacheValid(_balanceCache[cacheKey].timestamp))
        {
            var cachedBalance = _balanceCache[cacheKey].balance;
            Console.WriteLine($"├─ Cache Hit: Returning cached balance");
            Console.WriteLine($"└─ Cached Balance: {cachedBalance:F8} {cryptocurrency}\n");
            return cachedBalance;
        }

        // Step 3: Cache miss - delegate to real service
        Console.WriteLine($"├─ Cache Miss: Fetching from real service...\n");
        decimal balance = _realService.GetWalletBalance(walletAddress, cryptocurrency);

        // Step 4: Update cache
        _balanceCache[cacheKey] = (balance, DateTime.Now);
        Console.WriteLine($"[CryptoServiceProxy] Balance cached for future requests\n");

        return balance;
    }

    /// <summary>
    /// Proxy implementation of GetCryptoPrice.
    /// 1. Checks authentication
    /// 2. Checks cache first (prices change frequently, so shorter TTL)
    /// 3. Delegates to real service if cache miss
    /// 4. Updates cache
    /// </summary>
    public decimal GetCryptoPrice(string cryptocurrency)
    {
        Console.WriteLine($"\n┌─ [CryptoServiceProxy] GetCryptoPrice Request");
        Console.WriteLine($"├─ Cryptocurrency: {cryptocurrency}");

        // Step 1: Check access
        if (!CheckAccess("GetCryptoPrice"))
        {
            Console.WriteLine($"└─ [CryptoServiceProxy] Request BLOCKED\n");
            throw new UnauthorizedAccessException("API key validation failed.");
        }

        // Step 2: Check cache
        if (_priceCache.ContainsKey(cryptocurrency) && IsCacheValid(_priceCache[cryptocurrency].timestamp))
        {
            var cachedPrice = _priceCache[cryptocurrency].price;
            Console.WriteLine($"├─ Cache Hit: Returning cached price");
            Console.WriteLine($"└─ Cached Price: ${cachedPrice:F2}\n");
            return cachedPrice;
        }

        // Step 3: Cache miss - delegate to real service
        Console.WriteLine($"├─ Cache Miss: Fetching from real service...\n");
        decimal price = _realService.GetCryptoPrice(cryptocurrency);

        // Step 4: Update cache
        _priceCache[cryptocurrency] = (price, DateTime.Now);
        Console.WriteLine($"[CryptoServiceProxy] Price cached for future requests\n");

        return price;
    }
}
