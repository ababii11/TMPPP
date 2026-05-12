namespace CryptoArbitrage.Engine.DesignPatterns.Structural.Facade;

/// <summary>
/// Facade that aggregates subsystem classes behind a single simplified operation.
/// </summary>
public class ArbitrageExecutionFacade
{
    private readonly OpportunityScannerService _scannerService;
    private readonly LiquidityValidationService _liquidityService;
    private readonly OrderPlacementService _orderService;
    private readonly TradeAuditService _auditService;
    private readonly AdditionalArbitrageFacade? _optionalAdditionalFacade;

    public ArbitrageExecutionFacade(
        OpportunityScannerService scannerService,
        LiquidityValidationService liquidityService,
        OrderPlacementService orderService,
        TradeAuditService auditService,
        AdditionalArbitrageFacade? optionalAdditionalFacade = null)
    {
        _scannerService = scannerService;
        _liquidityService = liquidityService;
        _orderService = orderService;
        _auditService = auditService;
        _optionalAdditionalFacade = optionalAdditionalFacade;
    }

    public string SubsystemOperation(string traderId, string pair, decimal amount)
    {
        var spread = _scannerService.ScanSpreadPercent(pair);
        var hasLiquidity = _liquidityService.HasSufficientLiquidity(pair, amount);

        if (spread <= 0m || !hasLiquidity)
        {
            return "Facade Result: Opportunity rejected by subsystem checks.";
        }

        if (_optionalAdditionalFacade != null)
        {
            var advancedGatePassed = _optionalAdditionalFacade.AnotherOperation(pair, amount);

            if (!advancedGatePassed)
            {
                return "Facade Result: Opportunity blocked by additional facade controls.";
            }
        }

        var executionId = _orderService.PlaceArbitrageOrder(traderId, pair, amount);
        _auditService.Record(executionId, $"Pair={pair}; Amount={amount}; Spread={spread}%");

        return $"Facade Result: Executed arbitrage order {executionId} for {pair}.";
    }
}
