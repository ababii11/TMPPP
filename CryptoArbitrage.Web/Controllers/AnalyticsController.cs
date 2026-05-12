using Microsoft.AspNetCore.Mvc;

namespace CryptoArbitrage.Web.Controllers;

public class AnalyticsController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        if (HttpContext.Session.GetString("UserId") == null)
        {
            return RedirectToAction("Index", "Login");
        }

        ViewData["Title"] = "Analytics & Monitoring - CryptoManager";
        ViewData["Username"] = HttpContext.Session.GetString("Username");
        return View();
    }
}
