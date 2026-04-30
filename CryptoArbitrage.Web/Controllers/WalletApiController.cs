using Microsoft.AspNetCore.Mvc;
using CryptoArbitrage.Web.Models;
using CryptoArbitrage.Engine.DesignPatterns.Behavioral.Memento;
using CryptoArbitrage.Web.Services.Persistence;

namespace CryptoArbitrage.Web.Controllers;

[ApiController]
[Route("api/wallet")]
public class WalletApiController : ControllerBase
{
    private readonly ArbitrageBotStateOriginator _botState;
    private readonly StateManager _stateManager;
    private readonly IBotStateStore _persistence;

    public WalletApiController(
        ArbitrageBotStateOriginator botState,
        StateManager stateManager,
        IBotStateStore persistence)
    {
        _botState = botState;
        _stateManager = stateManager;
        _persistence = persistence;
    }

    [HttpGet("balance")]
    public IActionResult GetBalance()
    {
        return Ok(_botState.GetBalance());
    }

    [HttpPost("transaction")]
    public async Task<IActionResult> SendTransaction([FromBody] WalletTransactionRequest request)
    {
        if (request is null)
        {
            return BadRequest(new { message = "Request body is required." });
        }

        if (string.IsNullOrWhiteSpace(request.From) ||
            string.IsNullOrWhiteSpace(request.To) ||
            string.IsNullOrWhiteSpace(request.CryptoType) ||
            request.Amount <= 0)
        {
            return BadRequest(new { message = "Invalid transaction payload." });
        }

        var result = _botState.AddWalletTransaction(
            request.From.Trim(),
            request.To.Trim(),
            request.Amount,
            request.CryptoType.Trim());

        var snapshot = _stateManager.Save(_botState, "wallet-transaction");
        await _persistence.PersistSnapshotAsync(snapshot);

        return Ok(result);
    }

    [HttpGet("history")]
    public IActionResult GetHistory()
    {
        return Ok(_botState.GetWalletHistory());
    }
}
