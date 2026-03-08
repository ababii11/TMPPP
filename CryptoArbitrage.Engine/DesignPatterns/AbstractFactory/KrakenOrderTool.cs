namespace CryptoArbitrage.Engine.DesignPatterns.AbstractFactory;

/// <summary>
/// ConcreteProductA2 - Kraken implementation of order tool
/// Equivalent to ConcreteProductA2 from the diagram
/// </summary>
public class KrakenOrderTool : IOrderTool
{
    public string ExchangeName => "Kraken";

    public string PlaceBuyOrder(string cryptoPair, decimal amount, decimal price)
    {
        string orderId = $"KRK_BUY_{DateTime.Now.Ticks}";
        return $@"[KRAKEN] Buy Order Placed:
Order ID: {orderId}
Pair: {cryptoPair}
Amount: {amount}
Price: ${price:F4}
Fee: 0.16% (Kraken maker)
Status: OPEN
Exchange API: Kraken REST v0";
    }

    public string PlaceSellOrder(string cryptoPair, decimal amount, decimal price)
    {
        string orderId = $"KRK_SELL_{DateTime.Now.Ticks}";
        return $@"[KRAKEN] Sell Order Placed:
Order ID: {orderId}
Pair: {cryptoPair}
Amount: {amount}
Price: ${price:F4}
Fee: 0.16% (Kraken maker)
Status: OPEN
Exchange API: Kraken REST v0";
    }

    public string CancelOrder(string orderId)
    {
        return $@"[KRAKEN] Order Cancelled:
Order ID: {orderId}
Status: CANCELLED
Timestamp: {DateTime.Now}
Refund: Processing via Kraken API";
    }
}