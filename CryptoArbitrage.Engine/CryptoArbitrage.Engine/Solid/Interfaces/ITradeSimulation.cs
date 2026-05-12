using ArbitrageProject.Models;

namespace ArbitrageProject.Interfaces;

public interface ITradeSimulation
{
    decimal Simulate(CryptoPrice cryptoPrice, decimal investAmount);
}