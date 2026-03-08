namespace CryptoArbitrage.Engine.DesignPatterns.FactoryMethod;

/// <summary>
/// ConcreteProductB - Risk Analysis Report implementation  
/// </summary>
public class RiskAnalysisReport : IAnalysisReport
{
    private readonly decimal _priceVolatility;
    private readonly decimal _liquidityScore;
    private readonly string _exchangeName;
    private readonly string _cryptoPair;

    public string ReportType => "Risk Analysis";

    public RiskAnalysisReport(decimal priceVolatility, decimal liquidityScore, string exchangeName, string cryptoPair)
    {
        _priceVolatility = priceVolatility;
        _liquidityScore = liquidityScore;
        _exchangeName = exchangeName;
        _cryptoPair = cryptoPair;
    }

    /// <summary>
    /// Implementation of doStuff() - generates risk analysis report
    /// </summary>
    public string GenerateReport()
    {
        var riskLevel = CalculateRiskLevel();
        var recommendation = GetRecommendation(riskLevel);
        
        return $@"
=== RISK ANALYSIS REPORT ===
Crypto Pair: {_cryptoPair}
Exchange: {_exchangeName}
Price Volatility: {_priceVolatility:F2}%
Liquidity Score: {_liquidityScore:F2}/10
Risk Level: {riskLevel}
Recommendation: {recommendation}
Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
============================";
    }

    private string CalculateRiskLevel()
    {
        if (_priceVolatility > 15 || _liquidityScore < 3)
            return "HIGH";
        else if (_priceVolatility > 8 || _liquidityScore < 6)
            return "MEDIUM";
        else
            return "LOW";
    }

    private string GetRecommendation(string riskLevel)
    {
        return riskLevel switch
        {
            "HIGH" => "Avoid trading - High risk detected",
            "MEDIUM" => "Trade with caution - Monitor closely",
            "LOW" => "Safe to trade - Low risk conditions",
            _ => "Unknown risk level"
        };
    }
}