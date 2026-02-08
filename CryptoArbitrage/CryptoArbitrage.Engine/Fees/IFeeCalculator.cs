namespace ArbitrageApp.Fees
{
    public interface IFeeCalculator
    {
        decimal CalculateFee(decimal amount);
    }
}