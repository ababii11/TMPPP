namespace ArbitrageProject.Interfaces;

public interface IArbitrageStrategy
{
    bool IsProfitable(decimal buyPrice, decimal sellPrice);

    decimal CalculateProfit(decimal buyPrice, decimal sellPrice);
}