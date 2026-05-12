namespace CryptoArbitrage.Engine.DesignPatterns.FactoryMethod;

public class ProfitReportCreator : AnalysisReportCreator
{
    private readonly decimal _buyPrice;
    private readonly decimal _sellPrice;
    private readonly decimal _volume;
    private readonly string _cryptoPair;

    public ProfitReportCreator(decimal buyPrice, decimal sellPrice, decimal volume, string cryptoPair)
    {
        _buyPrice = buyPrice;
        _sellPrice = sellPrice;
        _volume = volume;
        _cryptoPair = cryptoPair;
    }

    public override IAnalysisReport CreateReport()
    {
        return new ProfitAnalysisReport(_buyPrice, _sellPrice, _volume, _cryptoPair);
    }
}