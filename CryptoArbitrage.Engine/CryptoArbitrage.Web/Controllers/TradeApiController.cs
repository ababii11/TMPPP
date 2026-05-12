using Microsoft.AspNetCore.Mvc;
using CryptoArbitrage.Web.Models;
using CryptoArbitrage.Engine.DesignPatterns.AbstractFactory;
using CryptoArbitrage.Engine.DesignPatterns.Behavioral.Command;
using CryptoArbitrage.Engine.DesignPatterns.Behavioral.Memento;
using CryptoArbitrage.Web.Services.Persistence;

namespace CryptoArbitrage.Web.Controllers;

[ApiController]
[Route("api/trade")]
public class TradeApiController : ControllerBase
{
    private readonly TradingBotInvoker _invoker;
    private readonly ArbitrageBotStateOriginator _botState;
    private readonly StateManager _stateManager;
    private readonly IBotStateStore _persistence;
    private static readonly object Sync = new();

    public TradeApiController(
        TradingBotInvoker invoker,
        ArbitrageBotStateOriginator botState,
        StateManager stateManager,
        IBotStateStore persistence)
    {
        _invoker = invoker;
        _botState = botState;
        _stateManager = stateManager;
        _persistence = persistence;
    }

    [HttpPost("execute")]
    public async Task<IActionResult> Execute([FromBody] TradeCommandRequest request)
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

        var snapshot = _stateManager.Save(_botState, $"trade-{side}");
        await _persistence.PersistSnapshotAsync(snapshot);

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
    public async Task<IActionResult> Undo()
    {
        string undoResult;
        int history;

        lock (Sync)
        {
            undoResult = _invoker.UndoLastCommand();
            history = _invoker.HistoryCount();
        }

        _botState.RecordUndo(undoResult);

    var snapshot = _stateManager.Save(_botState, "trade-undo");
    await _persistence.PersistSnapshotAsync(snapshot);

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
