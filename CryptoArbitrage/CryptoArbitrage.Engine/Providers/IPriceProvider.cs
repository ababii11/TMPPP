using ArbitrageApp.Models;

namespace ArbitrageApp.Providers
{
    public interface IPriceProvider
    {
        void FetchPrices();
        IReadOnlyList<CryptoPrice> Prices { get; }
    }
}