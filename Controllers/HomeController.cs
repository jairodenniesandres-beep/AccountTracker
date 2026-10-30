using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AccountTracker.Models;

namespace AccountTracker.Controllers;

public class HomeController : Controller
{
    // Accounts Index is the home page; keep /Home working for old links.
    [Authorize]
    public IActionResult Index() => RedirectToAction("Index", "Accounts");

    [AllowAnonymous]
    public IActionResult Privacy() => View();

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
