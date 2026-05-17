// ==========================================================================
//  Green Logistics Management System (GLMS) - Part 3 SOA  |  PROG7311
//  File: GreenLogisticsMS.Web/Controllers/ContractsController.cs
//  ----------------------------------------------------------------------
//  MVC controller for Contracts - delegates to the Web API via IApiClient.
//  Includes the search/filter screen and the status workflow (PATCH).
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
    public class ContractsController : Controller
    {
        private readonly IApiClient _api;

        public ContractsController(IApiClient api) => _api = api;

        public async Task<IActionResult> Index() => View(await _api.GetContractsAsync());

        public async Task<IActionResult> Search(ContractFilterModel filter)
        {
            filter.Results = await _api.GetContractsAsync(filter);
            return View(filter);
        }

        public async Task<IActionResult> Details(int id)
        {
            var contract = await _api.GetContractAsync(id);
            return contract == null ? NotFound() : View(contract);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateClientsAsync();
            return View(new ContractModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContractModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateClientsAsync(model.ClientId);
                return View(model);
            }
            if (await _api.CreateContractAsync(model))
            {
                TempData["Success"] = "Contract created.";
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", "The API rejected the contract (check the client and dates).");
            await PopulateClientsAsync(model.ClientId);
            return View(model);
        }

        // Status workflow - approve / hold / expire (PATCH on the API).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, ContractStatus status)
        {
            if (await _api.UpdateContractStatusAsync(id, status))
                TempData["Success"] = $"Contract #{id} set to {status}.";
            else
                TempData["Error"] = $"Could not change contract #{id} to {status}.";
            return RedirectToAction(nameof(Details), new { id });
        }

        public async Task<IActionResult> Delete(int id)
        {
            var contract = await _api.GetContractAsync(id);
            return contract == null ? NotFound() : View(contract);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _api.DeleteContractAsync(id);
            TempData["Success"] = "Contract deleted.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateClientsAsync(int? selected = null)
        {
            var clients = await _api.GetClientsAsync();
            ViewBag.Clients = new SelectList(clients, "Id", "Name", selected);
        }
    }
}
