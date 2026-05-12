namespace CryptoArbitrage.Engine.DesignPatterns.Structural.Composite;

/// <summary>
/// Common component interface for both leaf nodes and composite groups.
/// </summary>
public interface IPortfolioComponent
{
    string Execute();
}
