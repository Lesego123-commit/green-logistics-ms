// ==========================================================================
//  Green Logistics Management System (GLMS) - Part 3 SOA  |  PROG7311
//  File: GreenLogisticsMS.Web/Controllers/AccountController.cs
//  ----------------------------------------------------------------------
//  Handles login/logout. On successful login the JWT returned by the API is
//  stored in session and re-used by ApiClient for subsequent calls.
//  ----------------------------------------------------------------------
//  References (IIE Harvard):
//   - Microsoft. 2024. Overview of ASP.NET Core MVC. [Online]. Available at:
//     https://learn.microsoft.com/en-us/aspnet/core/mvc/overview [Accessed 31 May 2026].
//   - Microsoft. 2024. Session and state management in ASP.NET Core. [Online].
//     Available at: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state [Accessed 31 May 2026].
// ==========================================================================

using GreenLogisticsMS.Web.Models;
using GreenLogisticsMS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace GreenLogisticsMS.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IApiClient _api;

        public AccountController(IApiClient api) => _api = api;

        [HttpGet]
        public IActionResult Login() => View(new LoginModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var auth = await _api.LoginAsync(model);
            if (auth == null)
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }

            HttpContext.Session.SetString("JWToken", auth.Token);
            HttpContext.Session.SetString("Username", auth.Username);
            HttpContext.Session.SetString("Role", auth.Role);
            TempData["Success"] = $"Welcome, {auth.Username}.";
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "You have been signed out.";
            return RedirectToAction("Login");
        }
    }
}
