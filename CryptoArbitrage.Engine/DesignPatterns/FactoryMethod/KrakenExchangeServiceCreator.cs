using ArbitrageProject.Interfaces;
using ArbitrageProject.Services;

namespace CryptoArbitrage.Engine.DesignPatterns.FactoryMethod
{
   
    public sealed class KrakenExchangeServiceCreator : ExchangeServiceCreator
    {
        public KrakenExchangeServiceCreator() : base("KrakenCreator") { }

        protected override IExchangeService CreateExchangeService()
        {
            return new KrakenService();
        }
    }
}