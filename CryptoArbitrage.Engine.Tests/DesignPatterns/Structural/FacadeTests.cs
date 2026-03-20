using CryptoArbitrage.Engine.DesignPatterns.Structural.Facade;

namespace CryptoArbitrage.Engine.Tests.DesignPatterns.Structural;

public class FacadeTests
{
    [Fact]
    public void ExecuteOpportunity_ReturnsExecutedResult_WhenFlowIsValid()
    {
        var facade = new ArbitrageExecutionFacade(
            new OpportunityScannerService(),
            new LiquidityValidationService(),
            new OrderPlacementService(),
            new TradeAuditService());

        var result = facade.ExecuteOpportunity("TRADER-01", "BTC/USDT", 0.2m, 1.0m);

        Assert.True(result.IsExecuted);
        Assert.StartsWith("ARB-", result.ExecutionId);
    }

    [Fact]
    public void ExecuteOpportunity_ReturnsRejectedResult_WhenRiskThresholdIsTooLow()
    {
        var facade = new ArbitrageExecutionFacade(
            new OpportunityScannerService(),
            new LiquidityValidationService(),
            new OrderPlacementService(),
            new TradeAuditService());

        var result = facade.ExecuteOpportunity("TRADER-01", "BTC/USDT", 0.2m, 0.01m);

        Assert.False(result.IsExecuted);
        Assert.Equal("N/A", result.ExecutionId);
    }
}