namespace CryptoArbitrage.Engine.DesignPatterns.Structural.Composite;

/// <summary>
/// Leaf object in the portfolio hierarchy.
/// </summary>
public class SpotPosition : IPortfolioComponent
{
    public SpotPosition(string symbol, decimal quantity, decimal entryPriceUsd)
    {
        Symbol = symbol;
        Quantity = quantity;
        EntryPriceUsd = entryPriceUsd;
    }

    public string Symbol { get; }
    public decimal Quantity { get; }
    public decimal EntryPriceUsd { get; }

    public string Execute()
    {
        var totalUsd = Quantity * EntryPriceUsd;
        return $"Leaf Position -> {Symbol}: {Quantity} @ ${EntryPriceUsd} = ${totalUsd}";
    }
}
