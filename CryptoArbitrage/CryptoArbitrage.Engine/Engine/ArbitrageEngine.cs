using ArbitrageApp.Fees;
using ArbitrageApp.Strategies;
using System;

namespace ArbitrageApp.Engine
{
    public class ArbitrageEngine
    {
        private readonly IArbitrageStrategy _strategy;
        private readonly IFeeCalculator _feeCalculator;

        public ArbitrageEngine(IArbitrageStrategy strategy, IFeeCalculator feeCalculator)
        {
            _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
            _feeCalculator = feeCalculator ?? throw new ArgumentNullException(nameof(feeCalculator));
        }

        // Returns expected profit per unit after fees. If not profitable, returns 0 or negative value.
        public decimal CheckOpportunity(decimal buy, decimal sell)
        {
            if (!_strategy.IsProfitable(buy, sell))
                return 0m;

            // Calculate gross profit per unit
            var gross = sell - buy;

            // Subtract fees on buy and sell amounts
            var feeOnBuy = _feeCalculator.CalculateFee(buy);
            var feeOnSell = _feeCalculator.CalculateFee(sell);

            var net = gross - feeOnBuy - feeOnSell;
            return net;
        }
    }
}