namespace CryptoArbitrage.Engine.DesignPatterns.AbstractFactory;

/// <summary>
/// ConcreteProductA1 - Binance implementation of order tool
/// Equivalent to ConcreteProductA1 from the diagram
/// </summary>
public class BinanceOrderTool : IOrderTool
{
    public string ExchangeName => "Binance";

    public string PlaceBuyOrder(string cryptoPair, decimal amount, decimal price)
    {
        string orderId = $"BIN_BUY_{DateTime.Now.Ticks}";
        return $@"[BINANCE] Buy Order Placed:
Order ID: {orderId}
Pair: {cryptoPair}
Amount: {amount}
Price: ${price:F4}
Fee: 0.1% (Binance standard)
Status: PENDING
Exchange API: Binance REST v3";
    }

    public string PlaceSellOrder(string cryptoPair, decimal amount, decimal price)
    {
        string orderId = $"BIN_SELL_{DateTime.Now.Ticks}";
        return $@"[BINANCE] Sell Order Placed:
Order ID: {orderId}
Pair: {cryptoPair}
Amount: {amount}
Price: ${price:F4}
Fee: 0.1% (Binance standard)
Status: PENDING
Exchange API: Binance REST v3";
    }

    public string CancelOrder(string orderId)
    {
        return $@"[BINANCE] Order Cancelled:
Order ID: {orderId}
Status: CANCELLED
Timestamp: {DateTime.Now}
Refund: Processing via Binance API";
    }
}