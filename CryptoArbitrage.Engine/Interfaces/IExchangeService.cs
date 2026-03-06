namespace ArbitrageProject.Interfaces
{
    public interface IExchangeService
    {
        decimal GetPrice(string symbol);
        string GetName();
    }
}