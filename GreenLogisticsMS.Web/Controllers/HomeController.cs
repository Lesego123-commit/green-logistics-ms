// ==========================================================================
//  Green Logistics Management System (GLMS) - Part 3 SOA  |  PROG7311
//  File: GreenLogisticsMS.Web/Controllers/HomeController.cs
//  ----------------------------------------------------------------------
//  References (IIE Harvard):
//   - Microsoft. 2024. Overview of ASP.NET Core MVC. [Online]. Available at:
//     https://learn.microsoft.com/en-us/aspnet/core/mvc/overview [Accessed 31 May 2026].
// ==========================================================================

using System.Diagnostics;
using GreenLogisticsMS.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace GreenLogisticsMS.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() =>
            View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
