using Microsoft.AspNetCore.Mvc;
using CryptoArbitrage.Web.Models;
using CryptoArbitrage.Engine.DesignPatterns.Behavioral.Memento;

namespace CryptoArbitrage.Web.Controllers;

[ApiController]
[Route("api/wallet")]
public class WalletApiController : ControllerBase
{
    private readonly ArbitrageBotStateOriginator _botState;

    public WalletApiController(ArbitrageBotStateOriginator botState)
    {
        _botState = botState;
    }

    [HttpGet("balance")]
    public IActionResult GetBalance()
    {
        return Ok(_botState.GetBalance());
    }

    [HttpPost("transaction")]
    public IActionResult SendTransaction([FromBody] WalletTransactionRequest request)
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

        return Ok(result);
    }

    [HttpGet("history")]
    public IActionResult GetHistory()
    {
        return Ok(_botState.GetWalletHistory());
    }
}
