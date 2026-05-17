// ==========================================================================
//  Green Logistics Management System (GLMS) - Part 3 SOA  |  PROG7311
//  File: GreenLogisticsMS.Web/Services/ApiClient.cs
//  ----------------------------------------------------------------------
//  Typed HttpClient wrapper. Reads the JWT from the user's session and
//  attaches it as a Bearer token on every authenticated request. This is
//  how the MVC presentation layer consumes the Web API service layer.
//  ----------------------------------------------------------------------
//  References (IIE Harvard):
//   - Microsoft. 2024. Make HTTP requests using IHttpClientFactory. [Online].
//     Available at: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests [Accessed 31 May 2026].
//   - Microsoft. 2024. Session and state management in ASP.NET Core. [Online].
//     Available at: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state [Accessed 31 May 2026].
//   - Microsoft. 2024. How to serialize and deserialize JSON. [Online]. Available at:
//     https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/ [Accessed 31 May 2026].
// ==========================================================================

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using GreenLogisticsMS.Web.Models;

namespace GreenLogisticsMS.Web.Services
{
    public class ApiClient : IApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };

        public ApiClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
        }

        // ── Attach the stored JWT (if any) to the outgoing request ────────
        private void AddAuth()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWToken");
            _http.DefaultRequestHeaders.Authorization =
                string.IsNullOrEmpty(token) ? null : new AuthenticationHeaderValue("Bearer", token);
        }

        // ── Auth ──────────────────────────────────────────────────────────
        public async Task<AuthResponse?> LoginAsync(LoginModel model)
        {
            var resp = await _http.PostAsJsonAsync("api/auth/login", model);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<AuthResponse>(JsonOpts);
        }

        // ── Clients ─────────────────────────────────────────────────────────
        public async Task<List<ClientModel>> GetClientsAsync()
        {
            AddAuth();
            var resp = await _http.GetAsync("api/clients");
            if (!resp.IsSuccessStatusCode) return new();
            return await resp.Content.ReadFromJsonAsync<List<ClientModel>>(JsonOpts) ?? new();
        }

        public async Task<ClientModel?> GetClientAsync(int id)
        {
            AddAuth();
            var resp = await _http.GetAsync($"api/clients/{id}");
            return resp.IsSuccessStatusCode
                ? await resp.Content.ReadFromJsonAsync<ClientModel>(JsonOpts)
                : null;
        }

        public async Task<bool> CreateClientAsync(ClientModel model)
        {
            AddAuth();
            var resp = await _http.PostAsJsonAsync("api/clients", model);
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateClientAsync(ClientModel model)
        {
            AddAuth();
            var resp = await _http.PutAsJsonAsync($"api/clients/{model.Id}", model);
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteClientAsync(int id)
        {
            AddAuth();
            var resp = await _http.DeleteAsync($"api/clients/{id}");
            return resp.IsSuccessStatusCode;
        }

        // ── Contracts ───────────────────────────────────────────────────────
        public async Task<List<ContractModel>> GetContractsAsync(ContractFilterModel? filter = null)
        {
            AddAuth();
            var query = new List<string>();
            if (filter?.Status != null) query.Add($"status={filter.Status}");
            if (filter?.ClientId != null) query.Add($"clientId={filter.ClientId}");
            if (filter?.StartFrom != null) query.Add($"startFrom={filter.StartFrom:yyyy-MM-dd}");
            if (filter?.StartTo != null) query.Add($"startTo={filter.StartTo:yyyy-MM-dd}");
            var url = "api/contracts" + (query.Count > 0 ? "?" + string.Join("&", query) : "");

            var resp = await _http.GetAsync(url);
            if (!resp.IsSuccessStatusCode) return new();
            return await resp.Content.ReadFromJsonAsync<List<ContractModel>>(JsonOpts) ?? new();
        }

        public async Task<ContractModel?> GetContractAsync(int id)
        {
            AddAuth();
            var resp = await _http.GetAsync($"api/contracts/{id}");
            return resp.IsSuccessStatusCode
                ? await resp.Content.ReadFromJsonAsync<ContractModel>(JsonOpts)
                : null;
        }

        public async Task<bool> CreateContractAsync(ContractModel model)
        {
            AddAuth();
            var body = new
            {
                model.ClientId,
                model.StartDate,
                model.EndDate,
                Status = model.Status.ToString(),
                ServiceLevel = model.ServiceLevel.ToString()
            };
            var resp = await _http.PostAsJsonAsync("api/contracts", body);
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateContractStatusAsync(int id, ContractStatus status)
        {
            AddAuth();
            var body = new { Status = status.ToString() };
            var resp = await _http.PatchAsJsonAsync($"api/contracts/{id}/status", body);
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteContractAsync(int id)
        {
            AddAuth();
            var resp = await _http.DeleteAsync($"api/contracts/{id}");
            return resp.IsSuccessStatusCode;
        }

        // ── Service requests ──────────────────────────────────────────────
        public async Task<List<ServiceRequestModel>> GetServiceRequestsAsync(int? contractId = null)
        {
            AddAuth();
            var url = "api/servicerequests" + (contractId.HasValue ? $"?contractId={contractId}" : "");
            var resp = await _http.GetAsync(url);
            if (!resp.IsSuccessStatusCode) return new();
            return await resp.Content.ReadFromJsonAsync<List<ServiceRequestModel>>(JsonOpts) ?? new();
        }

        public async Task<(bool ok, string? error)> CreateServiceRequestAsync(ServiceRequestModel model)
        {
            AddAuth();
            var body = new
            {
                model.ContractId,
                model.Description,
                RequestType = model.RequestType.ToString(),
                model.CostUSD
            };
            var resp = await _http.PostAsJsonAsync("api/servicerequests", body);
            if (resp.IsSuccessStatusCode) return (true, null);

            // Surface the API's validation message (e.g. expired-contract rule).
            var content = await resp.Content.ReadAsStringAsync();
            try
            {
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("message", out var msg))
                    return (false, msg.GetString());
            }
            catch { /* not JSON - fall through */ }
            return (false, "The request could not be created.");
        }

        public async Task<bool> DeleteServiceRequestAsync(int id)
        {
            AddAuth();
            var resp = await _http.DeleteAsync($"api/servicerequests/{id}");
            return resp.IsSuccessStatusCode;
        }
    }
}
