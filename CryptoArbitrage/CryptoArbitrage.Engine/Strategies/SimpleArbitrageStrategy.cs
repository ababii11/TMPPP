namespace ArbitrageApp.Strategies
{
    public class SimpleArbitrageStrategy : IArbitrageStrategy
    {
        private readonly decimal _minProfitPercent;

        public SimpleArbitrageStrategy(decimal minProfitPercent)
        {
            _minProfitPercent = minProfitPercent;
        }

        // Returns true if ((sell - buy) / buy) * 100 >= minProfitPercent
        public bool IsProfitable(decimal buy, decimal sell)
        {
            if (buy <= 0) return false;
            var percent = (sell - buy) / buy * 100m;
            return percent >= _minProfitPercent;
        }
    }
}