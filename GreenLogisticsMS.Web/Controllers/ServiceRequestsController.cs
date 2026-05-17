// ==========================================================================
//  Green Logistics Management System (GLMS) - Part 3 SOA  |  PROG7311
//  File: GreenLogisticsMS.Web/Controllers/ServiceRequestsController.cs
//  ----------------------------------------------------------------------
//  MVC controller for Service Requests - delegates to the Web API. Surfaces
//  the API's business-rule errors (e.g. requests blocked on expired
//  contracts) back to the user as validation messages.
//  ----------------------------------------------------------------------
//  References (IIE Harvard):
//   - Microsoft. 2024. Overview of ASP.NET Core MVC. [Online]. Available at:
//     https://learn.microsoft.com/en-us/aspnet/core/mvc/overview [Accessed 31 May 2026].
//   - Microsoft. 2024. Make HTTP requests using IHttpClientFactory. [Online].
//     Available at: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests [Accessed 31 May 2026].
// ==========================================================================

using GreenLogisticsMS.Web.Models;
using GreenLogisticsMS.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GreenLogisticsMS.Web.Controllers
{
    [RequireLogin]
    public class ServiceRequestsController : Controller
    {
        private readonly IApiClient _api;

        public ServiceRequestsController(IApiClient api) => _api = api;

        public async Task<IActionResult> Index() => View(await _api.GetServiceRequestsAsync());

        public async Task<IActionResult> Create(int? contractId)
        {
            await PopulateContractsAsync(contractId);
            return View(new ServiceRequestModel { ContractId = contractId ?? 0 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateContractsAsync(model.ContractId);
                return View(model);
            }

            var (ok, error) = await _api.CreateServiceRequestAsync(model);
            if (ok)
            {
                TempData["Success"] = "Service request created (cost converted to ZAR by the API).";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", error ?? "The request could not be created.");
            await PopulateContractsAsync(model.ContractId);
            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var all = await _api.GetServiceRequestsAsync();
            var sr = all.FirstOrDefault(x => x.Id == id);
            return sr == null ? NotFound() : View(sr);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _api.DeleteServiceRequestAsync(id);
            TempData["Success"] = "Service request deleted.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateContractsAsync(int? selected = null)
        {
            var contracts = await _api.GetContractsAsync();
            var items = contracts.Select(c => new
            {
                c.Id,
                Display = $"{c.ClientName} - Contract #{c.Id} ({c.Status})"
            });
            ViewBag.Contracts = new SelectList(items, "Id", "Display", selected);
        }
    }
}
