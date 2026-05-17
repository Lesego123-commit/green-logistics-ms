// ==========================================================================
//  Green Logistics Management System (GLMS) - Part 3 SOA  |  PROG7311
//  File: GreenLogisticsMS.Web/Controllers/ClientsController.cs
//  ----------------------------------------------------------------------
//  MVC controller for Clients. Contains NO database code - every operation
//  is delegated to the Web API through IApiClient (SOA presentation layer).
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

namespace GreenLogisticsMS.Web.Controllers
{
    [RequireLogin]
    public class ClientsController : Controller
    {
        private readonly IApiClient _api;

        public ClientsController(IApiClient api) => _api = api;

        public async Task<IActionResult> Index() => View(await _api.GetClientsAsync());

        public async Task<IActionResult> Details(int id)
        {
            var client = await _api.GetClientAsync(id);
            return client == null ? NotFound() : View(client);
        }

        public IActionResult Create() => View(new ClientModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClientModel model)
        {
            if (!ModelState.IsValid) return View(model);
            if (await _api.CreateClientAsync(model))
            {
                TempData["Success"] = $"Client '{model.Name}' created.";
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", "The API rejected the request.");
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var client = await _api.GetClientAsync(id);
            return client == null ? NotFound() : View(client);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ClientModel model)
        {
            if (!ModelState.IsValid) return View(model);
            if (await _api.UpdateClientAsync(model))
            {
                TempData["Success"] = "Client updated.";
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", "Update failed.");
            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var client = await _api.GetClientAsync(id);
            return client == null ? NotFound() : View(client);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _api.DeleteClientAsync(id);
            TempData["Success"] = "Client deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
