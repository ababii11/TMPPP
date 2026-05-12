namespace CryptoArbitrage.Engine.DesignPatterns.Structural.Facade;

public class OpportunityScannerService
{
    public decimal ScanSpreadPercent(string pair)
    {
        return pair switch
        {
            "BTC/USDT" => 0.20m,
            "ETH/USDT" => 0.14m,
            _ => 0.05m
        };
    }
}
