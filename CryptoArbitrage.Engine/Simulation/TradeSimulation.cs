using ArbitrageProject.Interfaces;
using ArbitrageProject.Models;

namespace ArbitrageProject.Simulation;

public class TradeSimulation : ITradeSimulation
{
    public decimal Simulate(CryptoPrice cryptoPrice, decimal investAmount)
    {
        if (cryptoPrice.Price <= 0 || investAmount <= 0) return 0m;

        // Exemplu simplu: câte unități poți cumpăra la prețul curent
        var units = investAmount / cryptoPrice.Price;
        return units;
    }
}