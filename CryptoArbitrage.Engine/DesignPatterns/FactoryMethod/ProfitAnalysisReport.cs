namespace CryptoArbitrage.Engine.DesignPatterns.FactoryMethod;

/// <summary>
/// ConcreteProductA - Profit Analysis Report implementation
/// </summary>
public class ProfitAnalysisReport : IAnalysisReport
{
    private readonly decimal _buyPrice;
    private readonly decimal _sellPrice;
    private readonly decimal _volume;
    private readonly string _cryptoPair;

    public string ReportType => "Profit Analysis";

    public ProfitAnalysisReport(decimal buyPrice, decimal sellPrice, decimal volume, string cryptoPair)
    {
        _buyPrice = buyPrice;
        _sellPrice = sellPrice;
        _volume = volume;
        _cryptoPair = cryptoPair;
    }

    /// <summary>
    /// Implementation of doStuff() - generates profit analysis report
    /// </summary>
    public string GenerateReport()
    {
        var grossProfit = (_sellPrice - _buyPrice) * _volume;
        var profitMargin = (_sellPrice - _buyPrice) / _buyPrice * 100;
        
        return $@"
=== PROFIT ANALYSIS REPORT ===
Crypto Pair: {_cryptoPair}
Buy Price: ${_buyPrice:F4}
Sell Price: ${_sellPrice:F4}
Volume: {_volume:F2}
Gross Profit: ${grossProfit:F2}
Profit Margin: {profitMargin:F2}%
Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
===============================";
    }
}