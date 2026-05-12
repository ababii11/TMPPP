namespace CryptoArbitrage.Engine.DesignPatterns.FactoryMethod;

/// <summary>
/// ConcreteCreatorB - Creates risk analysis reports
/// Equivalent to ConcreteCreatorB from the diagram
/// </summary>
public class RiskReportCreator : AnalysisReportCreator
{
    private readonly decimal _priceVolatility;
    private readonly decimal _liquidityScore;
    private readonly string _exchangeName;
    private readonly string _cryptoPair;

    public RiskReportCreator(decimal priceVolatility, decimal liquidityScore, string exchangeName, string cryptoPair)
    {
        _priceVolatility = priceVolatility;
        _liquidityScore = liquidityScore;
        _exchangeName = exchangeName;
        _cryptoPair = cryptoPair;
    }

    /// <summary>
    /// Factory method implementation - creates RiskAnalysisReport
    /// Equivalent to "return new ConcreteProductB()" from diagram
    /// </summary>
    public override IAnalysisReport CreateReport()
    {
        return new RiskAnalysisReport(_priceVolatility, _liquidityScore, _exchangeName, _cryptoPair);
    }
}