using ArbitrageApp.Engine;
using System;

namespace ArbitrageApp.Simulation
{
    public class TradeSimulation
    {
        private readonly ArbitrageEngine _engine;

        public decimal InvestAmount { get; set; }

        public TradeSimulation(ArbitrageEngine engine, decimal investAmount = 1000m)
        {
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
            InvestAmount = investAmount;
        }

        // Simulate buying at 'buy' and selling at 'sell' using InvestAmount.
        // Returns the net profit (absolute decimal).
        public decimal Simulate(decimal buy, decimal sell)
        {
            if (buy <= 0) return 0m;

            var profitPerUnit = _engine.CheckOpportunity(buy, sell);
            if (profitPerUnit <= 0) return 0m;

            var units = InvestAmount / buy;
            return units * profitPerUnit;
        }
    }
}