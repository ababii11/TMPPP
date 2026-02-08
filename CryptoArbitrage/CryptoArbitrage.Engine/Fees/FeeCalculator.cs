namespace ArbitrageApp.Fees
{
    public class FeeCalculator : IFeeCalculator
    {
        private readonly decimal _feePercent;

        public FeeCalculator(decimal feePercent)
        {
            _feePercent = feePercent;
        }

        // Return fee amount (absolute)
        public decimal CalculateFee(decimal amount)
        {
            return amount * _feePercent / 100m;
        }
    }
}