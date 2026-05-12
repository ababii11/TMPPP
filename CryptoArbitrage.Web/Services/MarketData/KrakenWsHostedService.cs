using System.Net.WebSockets;
using System.Text.Json;

namespace CryptoArbitrage.Web.Services.MarketData;

/// <summary>
/// Hosted service for real-time quotes from Kraken WebSocket.
/// Connects to wss://ws.kraken.com/ and subscribes to ticker for XBT/USD and ETH/USD.
/// </summary>
public class KrakenWsHostedService : BackgroundService
{
    private readonly IQuoteStore _quoteStore;
    private readonly ILogger<KrakenWsHostedService> _logger;
    private ClientWebSocket? _ws;
    private int _reconnectDelayMs = 1000;
    private const int MaxReconnectDelayMs = 30000;

    public KrakenWsHostedService(IQuoteStore quoteStore, ILogger<KrakenWsHostedService> logger)
    {
        _quoteStore = quoteStore;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConnectAndReadAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kraken WS error, reconnecting in {DelayMs}ms", _reconnectDelayMs);
                await Task.Delay(_reconnectDelayMs, stoppingToken);
                _reconnectDelayMs = Math.Min(_reconnectDelayMs * 2, MaxReconnectDelayMs);
            }
        }
    }

    private async Task ConnectAndReadAsync(CancellationToken stoppingToken)
    {
        var uri = "wss://ws.kraken.com/";
        _ws = new ClientWebSocket();

        try
        {
            await _ws.ConnectAsync(new Uri(uri), stoppingToken);
            _logger.LogInformation("Kraken WS connected");
            _reconnectDelayMs = 1000;

            // Send subscription
            var subscribeMsg = new
            {
                @event = "subscribe",
                pair = new[] { "XBT/USD", "ETH/USD" },
                subscription = new { name = "ticker" }
            };
            var json = System.Text.Json.JsonSerializer.Serialize(subscribeMsg);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            await _ws.SendAsync(new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text, true, stoppingToken);

            var buffer = new byte[8192];
            while (_ws.State == WebSocketState.Open && !stoppingToken.IsCancellationRequested)
            {
                var result = await _ws.ReceiveAsync(
                    new ArraySegment<byte>(buffer), stoppingToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure,
                        "Closing", stoppingToken);
                }
                else if (result.Count > 0)
                {
                    ProcessKrakenMessage(buffer, result.Count);
                }
            }
        }
        finally
        {
            _ws?.Dispose();
        }
    }

    private void ProcessKrakenMessage(byte[] buffer, int count)
    {
        try
        {
            var json = System.Text.Encoding.UTF8.GetString(buffer, 0, count);
            using var doc = JsonDocument.Parse(json);

            var root = doc.RootElement;

            // Skip non-array or control messages
            if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() < 4)
                return;

            // [channelId, {data}, "ticker", "XBT/USD"]
            var tickerDataEl = root[1];
            var typeEl = root[2];
            var pairEl = root[3];

            if (typeEl.GetString() != "ticker" || tickerDataEl.ValueKind != JsonValueKind.Object)
                return;

            var pair = pairEl.GetString() ?? "";
            var symbol = MapKrakenPair(pair);
            if (string.IsNullOrEmpty(symbol))
                return;

            // Extract ask and bid from data
            // "a": [ask, ...], "b": [bid, ...], ...
            if (!tickerDataEl.TryGetProperty("a", out var askArr) ||
                !tickerDataEl.TryGetProperty("b", out var bidArr))
                return;

            var askStr = askArr.GetArrayLength() > 0 ? askArr[0].GetString() : "";
            var bidStr = bidArr.GetArrayLength() > 0 ? bidArr[0].GetString() : "";

            if (!decimal.TryParse(askStr, out var ask) ||
                !decimal.TryParse(bidStr, out var bid))
                return;

            var ts = DateTime.UtcNow;
            var quote = new Quote("kraken", symbol, bid, ask, ts, pair);
            _quoteStore.UpsertQuote(quote);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse Kraken message");
        }
    }

    private static string? MapKrakenPair(string pair)
    {
        return pair.ToUpperInvariant() switch
        {
            "XBT/USD" => "BTC",
            "ETH/USD" => "ETH",
            _ => null
        };
    }
}
