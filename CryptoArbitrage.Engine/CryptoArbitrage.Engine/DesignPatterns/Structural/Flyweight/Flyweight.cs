namespace CryptoArbitrage.Engine.DesignPatterns.Structural.Flyweight;

/// <summary>
/// Flyweight object that stores repeating symbol state and receives unique market state at runtime.
/// </summary>
public class Flyweight
{
    public Flyweight(string repeatingState)
    {
        RepeatingState = repeatingState;
    }

    public string RepeatingState { get; }

    public string Operation(decimal buyPrice, decimal sellPrice, string exchangeBuy, string exchangeSell)
    {
        return $"{RepeatingState}: BUY {buyPrice:N2} on {exchangeBuy} | SELL {sellPrice:N2} on {exchangeSell}";
    }
}
