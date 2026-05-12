using Microsoft.AspNetCore.Mvc;

namespace CryptoArbitrage.Web.Controllers;

public class WalletController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        if (HttpContext.Session.GetString("UserId") == null)
        {
            return RedirectToAction("Index", "Login");
        }

        ViewData["Title"] = "Wallet - CryptoManager";
        ViewData["Username"] = HttpContext.Session.GetString("Username");
        return View();
    }
}
