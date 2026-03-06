using ArbitrageProject.Interfaces;
using ArbitrageProject.Services;

namespace CryptoArbitrage.Engine.DesignPatterns.FactoryMethod
{
   
    public sealed class BinanceExchangeServiceCreator : ExchangeServiceCreator
    {
        public BinanceExchangeServiceCreator() : base("BinanceCreator") { }

        protected override IExchangeService CreateExchangeService()
        {
            return new BinanceService();
        }
    }
}