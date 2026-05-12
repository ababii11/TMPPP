namespace CryptoArbitrage.Engine.DesignPatterns.Behavioral.Observer;

/// <summary>
/// Subject interface used to manage observers and broadcast updates.
/// </summary>
public interface IPriceSubject
{
    void Attach(IPriceObserver observer);

    void Detach(IPriceObserver observer);

    void Notify(string symbol);
}
