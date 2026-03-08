using ArbitrageProject.Interfaces;

namespace ArbitrageProject.Services;

public abstract class ExchangeService : IExchangeService
{
    private readonly string _name;

    protected ExchangeService(string name) => _name = name;

    public string GetName() => _name;

    public abstract decimal GetPrice(string symbol);
}
