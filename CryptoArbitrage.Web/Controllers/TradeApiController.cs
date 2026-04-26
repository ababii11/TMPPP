using Microsoft.AspNetCore.Mvc;
using CryptoArbitrage.Web.Models;
using CryptoArbitrage.Engine.DesignPatterns.AbstractFactory;
using CryptoArbitrage.Engine.DesignPatterns.Behavioral.Command;
using CryptoArbitrage.Engine.DesignPatterns.Behavioral.Memento;

namespace CryptoArbitrage.Web.Controllers;

[ApiController]
[Route("api/trade")]
public class TradeApiController : ControllerBase
{
    private readonly TradingBotInvoker _invoker;
    private readonly ArbitrageBotStateOriginator _botState;
    private static readonly object Sync = new();

    public TradeApiController(TradingBotInvoker invoker, ArbitrageBotStateOriginator botState)
    {
        _invoker = invoker;
        _botState = botState;
    }

    [HttpPost("execute")]
    public IActionResult Execute([FromBody] TradeCommandRequest request)
    {
        if (request is null)
        {
            return BadRequest(new { message = "Request body is required." });
        }

        if (string.IsNullOrWhiteSpace(request.Pair) || request.Amount <= 0m || request.Price <= 0m)
        {
            return BadRequest(new { message = "Invalid order payload." });
        }

        var side = (request.Side ?? "buy").Trim().ToLowerInvariant();
        if (side is not ("buy" or "sell"))
        {
            return BadRequest(new { message = "Side must be buy or sell." });
        }

        var receiver = ResolveReceiver(request.Exchange);
        ICommand command = side == "buy"
            ? new ExecuteBuyOrderCommand(receiver, request.Pair.Trim().ToUpperInvariant(), request.Amount, request.Price)
            : new ExecuteSellOrderCommand(receiver, request.Pair.Trim().ToUpperInvariant(), request.Amount, request.Price);

        string execution;
        int history;

        lock (Sync)
        {
            execution = _invoker.ExecuteCommand(command);
            history = _invoker.HistoryCount();
        }

        _botState.RecordTradeExecution(
            receiver.ExchangeName,
            side,
            request.Pair.Trim().ToUpperInvariant(),
            request.Amount,
            request.Price,
            execution);

        return Ok(new
        {
            success = true,
            side,
            exchange = receiver.ExchangeName,
            pair = request.Pair.Trim().ToUpperInvariant(),
            command = command.Name,
            execution,
            historyCount = history
        });
    }

    [HttpPost("undo")]
    public IActionResult Undo()
    {
        string undoResult;
        int history;

        lock (Sync)
        {
            undoResult = _invoker.UndoLastCommand();
            history = _invoker.HistoryCount();
        }

        _botState.RecordUndo(undoResult);

        return Ok(new
        {
            success = true,
            undoResult,
            historyCount = history
        });
    }

    private static IOrderTool ResolveReceiver(string? exchange)
    {
        return (exchange ?? "binance").Trim().ToLowerInvariant() switch
        {
            "kraken" => new KrakenOrderTool(),
            _ => new BinanceOrderTool()
        };
    }
}
