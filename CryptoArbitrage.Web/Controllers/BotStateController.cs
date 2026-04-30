using Microsoft.AspNetCore.Mvc;
using CryptoArbitrage.Web.Models;
using CryptoArbitrage.Engine.DesignPatterns.Behavioral.Command;
using CryptoArbitrage.Engine.DesignPatterns.Behavioral.Memento;
using CryptoArbitrage.Web.Services.Persistence;

namespace CryptoArbitrage.Web.Controllers;

[ApiController]
[Route("api/botstate")]
public class BotStateController : ControllerBase
{
    private readonly ArbitrageBotStateOriginator _originator;
    private readonly StateManager _stateManager;
    private readonly TradingBotInvoker _invoker;
    private readonly IBotStateStore _persistence;

    public BotStateController(
        ArbitrageBotStateOriginator originator,
        StateManager stateManager,
        TradingBotInvoker invoker,
        IBotStateStore persistence)
    {
        _originator = originator;
        _stateManager = stateManager;
        _invoker = invoker;
        _persistence = persistence;
    }

    [HttpGet("current")]
    public IActionResult Current()
    {
        return Ok(_originator.GetBotState());
    }

    [HttpGet("snapshots")]
    public IActionResult Snapshots()
    {
        return Ok(_stateManager.GetSnapshots());
    }

    [HttpPost("save")]
    public async Task<IActionResult> Save([FromBody] BotStateSaveRequest? request)
    {
        var label = string.IsNullOrWhiteSpace(request?.Label)
            ? $"snapshot-{DateTime.UtcNow:yyyyMMdd-HHmmss}"
            : request!.Label.Trim();

        var snapshot = _stateManager.Save(_originator, label);
        await _persistence.PersistSnapshotAsync(snapshot);

        return Ok(new
        {
            success = true,
            snapshotId = snapshot.SnapshotId,
            capturedAt = snapshot.CapturedAt.ToString("yyyy-MM-dd HH:mm:ss") + " UTC",
            label = snapshot.Label,
            activeStrategy = snapshot.ActiveStrategy,
            balance = snapshot.Balance
        });
    }

    [HttpPost("restore/{snapshotId}")]
    public async Task<IActionResult> Restore(string snapshotId)
    {
        if (string.IsNullOrWhiteSpace(snapshotId))
        {
            return BadRequest(new { message = "Snapshot id is required." });
        }

        var restored = _stateManager.Restore(_originator, snapshotId.Trim());
        if (!restored)
        {
            return NotFound(new { message = "Snapshot not found." });
        }

        // Undo stack no longer reflects restored open-trade state.
        _invoker.ClearHistory();

        var currentSnapshot = _stateManager.Save(_originator, $"restore-{snapshotId.Trim()}");
        await _persistence.PersistSnapshotAsync(currentSnapshot);

        return Ok(new
        {
            success = true,
            message = "Bot state restored.",
            snapshotId = snapshotId.Trim(),
            current = _originator.GetBotState()
        });
    }
}
