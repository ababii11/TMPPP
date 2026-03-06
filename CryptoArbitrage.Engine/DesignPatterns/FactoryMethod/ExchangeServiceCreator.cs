using ArbitrageProject.Interfaces;

namespace CryptoArbitrage.Engine.DesignPatterns.FactoryMethod;

public abstract class ExchangeServiceCreator
{
    public string CreatorName { get; }

    protected ExchangeServiceCreator(string creatorName)
    {
        CreatorName = creatorName;
    }

    public IExchangeService GetService()
    {
        return CreateExchangeService();
    }

    protected abstract IExchangeService CreateExchangeService();
}