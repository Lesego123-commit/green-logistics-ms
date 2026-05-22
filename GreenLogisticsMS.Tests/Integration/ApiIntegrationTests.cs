// ==========================================================================
//  Green Logistics Management System (GLMS) - Part 3 SOA  |  PROG7311
//  File: GreenLogisticsMS.Tests/Integration/ApiIntegrationTests.cs
//  ----------------------------------------------------------------------
//  Automated API integration tests (Part 3 brief requirement). These call
//  the *running* API endpoints over HTTP and assert on the real responses:
//    - protected endpoints return 401 without a token,
//    - login returns a JWT,
//    - GET /api/contracts returns 200 OK with non-null JSON,
//    - POST creates data, PATCH drives the status workflow, and the
//      expired-contract business rule is enforced.
//  ----------------------------------------------------------------------
//  References (IIE Harvard):
//   - Microsoft. 2024. Integration tests in ASP.NET Core. [Online]. Available at:
//     https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests [Accessed 31 May 2026].
//   - Microsoft. 2024. xUnit.net. [Online]. Available at: https://xunit.net/ [Accessed 31 May 2026].
//   - Humble, J. & Farley, D. 2010. Continuous Delivery. Boston: Addison-Wesley.
// ==========================================================================

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using GreenLogisticsMS.Api.DTOs;

namespace GreenLogisticsMS.Tests.Integration
{
    public class ApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };

        public ApiIntegrationTests(CustomWebApplicationFactory factory) => _factory = factory;

        // ── Helpers ─────────────────────────────────────────────────────
        private async Task<HttpClient> CreateAuthenticatedClientAsync()
        {
            var client = _factory.CreateClient();
            var login = await client.PostAsJsonAsync("/api/auth/login",
                new LoginDto { Username = "admin", Password = "Admin@123" });
            login.EnsureSuccessStatusCode();

            var auth = await login.Content.ReadFromJsonAsync<AuthResponseDto>(JsonOpts);
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", auth!.Token);
            return client;
        }

        // ── Health & Auth ───────────────────────────────────────────────
        [Fact]
        public async Task Health_ReturnsOk()
        {
            var client = _factory.CreateClient();
            var resp = await client.GetAsync("/health");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsJwt()
        {
            var client = _factory.CreateClient();
            var resp = await client.PostAsJsonAsync("/api/auth/login",
                new LoginDto { Username = "admin", Password = "Admin@123" });

            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
            var auth = await resp.Content.ReadFromJsonAsync<AuthResponseDto>(JsonOpts);
            Assert.NotNull(auth);
            Assert.False(string.IsNullOrWhiteSpace(auth!.Token));
        }

        [Fact]
        public async Task Login_WithBadPassword_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            var resp = await client.PostAsJsonAsync("/api/auth/login",
                new LoginDto { Username = "admin", Password = "wrong" });
            Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
        }

        [Fact]
        public async Task GetContracts_WithoutToken_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            var resp = await client.GetAsync("/api/contracts");
            Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
        }

        // ── Contracts: the brief's headline example ──────────────────────
        [Fact]
        public async Task GetContracts_WithToken_Returns200AndNonNullJson()
        {
            var client = await CreateAuthenticatedClientAsync();
            var resp = await client.GetAsync("/api/contracts");

            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
            var contracts = await resp.Content.ReadFromJsonAsync<List<ContractDto>>(JsonOpts);
            Assert.NotNull(contracts);          // returned JSON is not null
        }

        [Fact]
        public async Task GetContracts_FilterByStatus_ReturnsOnlyMatching()
        {
            var client = await CreateAuthenticatedClientAsync();
            var resp = await client.GetAsync("/api/contracts?status=Active");

            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
            var contracts = await resp.Content.ReadFromJsonAsync<List<ContractDto>>(JsonOpts);
            Assert.NotNull(contracts);
            Assert.All(contracts!, c => Assert.Equal(ContractStatusName.Active, c.Status.ToString()));
        }

        [Fact]
        public async Task PostContract_CreatesAndReturns201()
        {
            var client = await CreateAuthenticatedClientAsync();
            var dto = new ContractCreateDto
            {
                ClientId = 1,
                StartDate = new DateTime(2026, 2, 1),
                EndDate = new DateTime(2026, 11, 30)
            };

            var resp = await client.PostAsJsonAsync("/api/contracts", dto);
            Assert.Equal(HttpStatusCode.Created, resp.StatusCode);

            var created = await resp.Content.ReadFromJsonAsync<ContractDto>(JsonOpts);
            Assert.NotNull(created);
            Assert.True(created!.Id > 0);
        }

        [Fact]
        public async Task PatchContractStatus_UpdatesStatus()
        {
            var client = await CreateAuthenticatedClientAsync();

            // create a contract to mutate
            var create = await client.PostAsJsonAsync("/api/contracts", new ContractCreateDto
            {
                ClientId = 1,
                StartDate = new DateTime(2026, 3, 1),
                EndDate = new DateTime(2026, 9, 1)
            });
            var contract = await create.Content.ReadFromJsonAsync<ContractDto>(JsonOpts);

            var patch = await client.PatchAsJsonAsync(
                $"/api/contracts/{contract!.Id}/status",
                new ContractStatusUpdateDto { Status = Api.Models.ContractStatus.OnHold });

            Assert.Equal(HttpStatusCode.OK, patch.StatusCode);
            var updated = await patch.Content.ReadFromJsonAsync<ContractDto>(JsonOpts);
            Assert.Equal("OnHold", updated!.Status.ToString());
        }

        // ── Service requests: business rule enforced ─────────────────────
        [Fact]
        public async Task PostServiceRequest_OnExpiredContract_IsRejected()
        {
            var client = await CreateAuthenticatedClientAsync();

            // create then expire a contract
            var create = await client.PostAsJsonAsync("/api/contracts", new ContractCreateDto
            {
                ClientId = 1,
                StartDate = new DateTime(2025, 1, 1),
                EndDate = new DateTime(2025, 12, 31)
            });
            var contract = await create.Content.ReadFromJsonAsync<ContractDto>(JsonOpts);
            await client.PatchAsJsonAsync($"/api/contracts/{contract!.Id}/status",
                new ContractStatusUpdateDto { Status = Api.Models.ContractStatus.Expired });

            // attempt to raise a service request against it
            var resp = await client.PostAsJsonAsync("/api/servicerequests", new ServiceRequestCreateDto
            {
                ContractId = contract.Id,
                Description = "Should be blocked",
                RequestType = Api.Models.RequestType.Domestic,
                CostUSD = 100m
            });

            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task PostServiceRequest_OnActiveContract_ConvertsCurrencyAndCreates()
        {
            var client = await CreateAuthenticatedClientAsync();
            var resp = await client.PostAsJsonAsync("/api/servicerequests", new ServiceRequestCreateDto
            {
                ContractId = 1,            // seeded Active contract
                Description = "Standard domestic delivery",
                RequestType = Api.Models.RequestType.Domestic,
                CostUSD = 100m
            });

            Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
            var sr = await resp.Content.ReadFromJsonAsync<ServiceRequestDto>(JsonOpts);
            Assert.NotNull(sr);
            Assert.True(sr!.CostZAR > 0, "API should have converted USD to a positive ZAR amount.");
            Assert.True(sr.ExchangeRateUsed > 0);
        }

        // ── Clients ──────────────────────────────────────────────────────
        [Fact]
        public async Task GetClients_WithToken_Returns200AndNonNullJson()
        {
            var client = await CreateAuthenticatedClientAsync();
            var resp = await client.GetAsync("/api/clients");

            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
            var clients = await resp.Content.ReadFromJsonAsync<List<ClientDto>>(JsonOpts);
            Assert.NotNull(clients);
            Assert.NotEmpty(clients!);   // seeded client exists
        }

        private static class ContractStatusName
        {
            public const string Active = "Active";
        }
    }
}
