using ArbitrageProject.Interfaces;

namespace CryptoArbitrage.Engine.DesignPatterns.FactoryMethod
{
    public static class FactoryMethodDemo
    {
        public static string Run()
        {
            ExchangeServiceCreator creator = new BinanceExchangeServiceCreator();
            IExchangeService service = creator.GetService();

            var name = service.GetName();
            var btc = service.GetPrice("BTC");

            return $"FactoryMethod -> {name}, BTC={btc}";
        }
    }
}
