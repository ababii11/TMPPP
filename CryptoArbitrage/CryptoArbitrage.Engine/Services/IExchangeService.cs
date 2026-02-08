namespace ArbitrageApp.Services
{
    public interface IExchangeService
    {
        decimal GetPrice(string symbol);
        string GetName();
    }
}