using CryptoArbitrage.Engine.DesignPatterns.Structural.Composite;

namespace CryptoArbitrage.Engine.Tests.DesignPatterns.Structural;

public class CompositeTests
{
    [Fact]
    public void PortfolioGroup_GetUsdValue_ReturnsRecursiveSumOfChildren()
    {
        var tradingWallet = new PortfolioGroup("Trading Wallet");
        tradingWallet.Add(new SpotPosition("BTC", 0.1m, 65000m));
        tradingWallet.Add(new SpotPosition("ETH", 1m, 3000m));

        var vault = new PortfolioGroup("Vault");
        vault.Add(new SpotPosition("SOL", 100m, 150m));

        var root = new PortfolioGroup("Root Portfolio");
        root.Add(tradingWallet);
        root.Add(vault);

        Assert.Equal(24500m, root.GetUsdValue());
    }

    [Fact]
    public void PortfolioGroup_Display_ContainsAllLeafSymbols()
    {
        var root = new PortfolioGroup("Root Portfolio");
        root.Add(new SpotPosition("BTC", 0.2m, 64000m));
        root.Add(new SpotPosition("ETH", 1.5m, 3100m));

        var output = root.Display();

        Assert.Contains("BTC", output);
        Assert.Contains("ETH", output);
        Assert.Contains("Root Portfolio", output);
    }
}