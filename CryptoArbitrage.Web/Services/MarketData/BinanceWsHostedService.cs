using System.Globalization;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace CryptoArbitrage.Web.Services.MarketData;

/// <summary>
/// Hosted service for real-time quotes from Binance WebSocket.
/// Connects to combined stream: btcusdt@bookTicker, ethusdt@bookTicker
/// </summary>
public class BinanceWsHostedService : BackgroundService
{
    private readonly IQuoteStore _quoteStore;
    private readonly ILogger<BinanceWsHostedService> _logger;
    private ClientWebSocket? _ws;
    private int _reconnectDelayMs = 1000;
    private const int MaxReconnectDelayMs = 30000;
    private readonly StringBuilder _messageBuffer = new();

    public BinanceWsHostedService(IQuoteStore quoteStore, ILogger<BinanceWsHostedService> logger)
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
                _logger.LogError(ex, "Binance WS error, reconnecting in {DelayMs}ms", _reconnectDelayMs);
                await Task.Delay(_reconnectDelayMs, stoppingToken);
                _reconnectDelayMs = Math.Min(_reconnectDelayMs * 2, MaxReconnectDelayMs);
            }
        }
    }

    private async Task ConnectAndReadAsync(CancellationToken stoppingToken)
    {
        var uri = "wss://stream.binance.com:9443/stream?streams=btcusdt@bookTicker/ethusdt@bookTicker";
        _ws = new ClientWebSocket();

        try
        {
            await _ws.ConnectAsync(new Uri(uri), stoppingToken);
            _logger.LogInformation("Binance WS connected");
            _reconnectDelayMs = 1000;
            _messageBuffer.Clear();

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
                    // Accumulate message until EndOfMessage
                    var messageText = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    _messageBuffer.Append(messageText);

                    if (result.EndOfMessage)
                    {
                        ProcessBinanceMessage(_messageBuffer.ToString());
                        _messageBuffer.Clear();
                    }
                }
            }
        }
        finally
        {
            _ws?.Dispose();
        }
    }

    private void ProcessBinanceMessage(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("data", out var dataEl))
                return;

            var data = dataEl;
            if (!data.TryGetProperty("s", out var sEl) ||
                !data.TryGetProperty("b", out var bEl) ||
                !data.TryGetProperty("a", out var aEl) ||
                !data.TryGetProperty("E", out var eEl))
                return;

            var sourceSymbol = sEl.GetString() ?? "";
            
            // Parse bid/ask with InvariantCulture
            var bidStr = bEl.GetString() ?? "";
            var askStr = aEl.GetString() ?? "";
            if (!decimal.TryParse(bidStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var bid) ||
                !decimal.TryParse(askStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var ask))
                return;

            // Parse E as long (it's numeric in Binance JSON, not string)
            long tsMs;
            if (eEl.ValueKind == JsonValueKind.String)
            {
                // Handle string case
                if (!long.TryParse(eEl.GetString(), out tsMs))
                    return;
            }
            else if (eEl.ValueKind == JsonValueKind.Number)
            {
                // Handle numeric case
                if (!eEl.TryGetInt64(out tsMs))
                    return;
            }
            else
            {
                return;
            }

            var symbol = MapBinanceSymbol(sourceSymbol);
            if (string.IsNullOrEmpty(symbol))
                return;

            var ts = UnixTimeStampToUtc(tsMs);
            var quote = new Quote("binance", symbol, bid, ask, ts, sourceSymbol);
            _quoteStore.UpsertQuote(quote);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse Binance message");
        }
    }

    private static string? MapBinanceSymbol(string sourceSymbol)
    {
        return sourceSymbol.ToUpperInvariant() switch
        {
            "BTCUSDT" => "BTC",
            "ETHUSDT" => "ETH",
            _ => null
        };
    }

    private static DateTime UnixTimeStampToUtc(long ms)
    {
        return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            .AddMilliseconds(ms);
    }
}
