using ArbitrageProject.Interfaces;

namespace ArbitrageProject.Services;

public class FeeCalculator : IFeeCalculator
{
    private readonly decimal _feePercent;

    // feePercent is a percentage (e.g. 0.1m == 0.1%)
    public FeeCalculator(decimal feePercent = 0.1m) => _feePercent = feePercent;

    public decimal CalculateFee(decimal amount) => amount * _feePercent / 100m;
}
