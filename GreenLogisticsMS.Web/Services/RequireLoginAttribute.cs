// ==========================================================================
//  Green Logistics Management System (GLMS) - Part 3 SOA  |  PROG7311
//  File: GreenLogisticsMS.Web/Services/RequireLoginAttribute.cs
//  ----------------------------------------------------------------------
//  Lightweight action filter that redirects to the login page when no JWT
//  is present in session. Applied to the data-management controllers so the
//  whole client requires authentication (addresses Part 2 "no logging in").
//  ----------------------------------------------------------------------
//  References (IIE Harvard):
//   - Microsoft. 2024. Filters in ASP.NET Core. [Online]. Available at:
//     https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/filters [Accessed 31 May 2026].
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GreenLogisticsMS.Web.Services
{
    public class RequireLoginAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var token = context.HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token))
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
            }
            base.OnActionExecuting(context);
        }
    }
}
