namespace ArbitrageProject.Interfaces;

public interface IArbitrageStrategy
{
    // Decide based on gross prices whether an opportunity meets the strategy's threshold.
    bool IsProfitable(decimal buyPrice, decimal sellPrice);
}

