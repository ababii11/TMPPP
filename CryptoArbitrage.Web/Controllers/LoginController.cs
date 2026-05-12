using Microsoft.AspNetCore.Mvc;
using CryptoArbitrage.Web.Services.Auth;

namespace CryptoArbitrage.Web.Controllers;

public class LoginController : Controller
{
    private readonly IUserStore _userStore;

    public LoginController(IUserStore userStore)
    {
        _userStore = userStore;
    }

    [HttpGet]
    public IActionResult Index()
    {
        if (HttpContext.Session.GetString("UserId") != null)
        {
            return RedirectToAction("Index", "Dashboard");
        }
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string username, string password, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || username.Length < 3 || password.Length < 3)
        {
            ViewData["Error"] = "Invalid username or password. Use at least 3 characters.";
            return View("Index");
        }

        var existingUser = await _userStore.FindByUsernameAsync(username, cancellationToken);
        if (existingUser == null)
        {
            var createdUser = await _userStore.CreateUserAsync(username, password, cancellationToken);
            HttpContext.Session.SetString("UserId", createdUser.UserId.ToString());
            HttpContext.Session.SetString("Username", createdUser.Username);
            await _userStore.UpdateLastLoginAsync(createdUser.UserId, cancellationToken);
            return RedirectToAction("Index", "Dashboard");
        }

        var isValid = await _userStore.ValidateUserAsync(username, password, cancellationToken);
        if (!isValid)
        {
            ViewData["Error"] = "Incorrect password for this username.";
            return View("Index");
        }

        HttpContext.Session.SetString("UserId", existingUser.UserId.ToString());
        HttpContext.Session.SetString("Username", existingUser.Username);
        await _userStore.UpdateLastLoginAsync(existingUser.UserId, cancellationToken);
        return RedirectToAction("Index", "Dashboard");
    }

    [HttpGet]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index");
    }
}
