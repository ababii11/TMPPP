using System;

namespace ArbitrageApp.Services
{
    public abstract class ExchangeService : IExchangeService
    {
        protected readonly string name;

        protected ExchangeService(string name)
        {
            this.name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public virtual string GetName() => name;

        // Concrete exchanges must implement how to get price
        public abstract decimal GetPrice(string symbol);
    }
}