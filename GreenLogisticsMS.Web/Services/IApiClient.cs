// ==========================================================================
//  Green Logistics Management System (GLMS) - Part 3 SOA  |  PROG7311
//  File: GreenLogisticsMS.Web/Services/IApiClient.cs
//  ----------------------------------------------------------------------
//  Abstraction over the typed HttpClient that the MVC controllers use to
//  talk to the Web API. Keeps controllers free of HTTP plumbing.
//  ----------------------------------------------------------------------
//  References (IIE Harvard):
//   - Microsoft. 2024. Make HTTP requests using IHttpClientFactory (typed clients).
//     [Online]. Available at: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests [Accessed 31 May 2026].
// ==========================================================================

using GreenLogisticsMS.Web.Models;

namespace GreenLogisticsMS.Web.Services
{
    public interface IApiClient
    {
        // Auth
        Task<AuthResponse?> LoginAsync(LoginModel model);

        // Clients
        Task<List<ClientModel>> GetClientsAsync();
        Task<ClientModel?> GetClientAsync(int id);
        Task<bool> CreateClientAsync(ClientModel model);
        Task<bool> UpdateClientAsync(ClientModel model);
        Task<bool> DeleteClientAsync(int id);

        // Contracts
        Task<List<ContractModel>> GetContractsAsync(ContractFilterModel? filter = null);
        Task<ContractModel?> GetContractAsync(int id);
        Task<bool> CreateContractAsync(ContractModel model);
        Task<bool> UpdateContractStatusAsync(int id, ContractStatus status);
        Task<bool> DeleteContractAsync(int id);

        // Service requests
        Task<List<ServiceRequestModel>> GetServiceRequestsAsync(int? contractId = null);
        Task<(bool ok, string? error)> CreateServiceRequestAsync(ServiceRequestModel model);
        Task<bool> DeleteServiceRequestAsync(int id);
    }
}
