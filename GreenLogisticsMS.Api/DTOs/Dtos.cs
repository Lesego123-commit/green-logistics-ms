// ==========================================================================
//  Green Logistics Management System (GLMS) - Part 3 SOA  |  PROG7311
//  File: GreenLogisticsMS.Api/DTOs/Dtos.cs
//  ----------------------------------------------------------------------
//  Data Transfer Objects (DTOs). These define the JSON contract the API
//  exposes to clients, keeping the EF Core entities decoupled from the
//  public surface of the service (a core SOA principle).
//  ----------------------------------------------------------------------
//  References (IIE Harvard):
//   - Microsoft. 2024. Create web APIs with ASP.NET Core. [Online]. Available at:
//     https://learn.microsoft.com/en-us/aspnet/core/web-api/ [Accessed 31 May 2026].
//   - Fowler, M. 2002. Patterns of Enterprise Application Architecture. Boston:
//     Addison-Wesley. (Data Transfer Object pattern.)
//   - Microsoft. 2024. DTOs and ProjectTo. [Online]. Available at:
//     https://learn.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle [Accessed 31 May 2026].
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using GreenLogisticsMS.Api.Models;

namespace GreenLogisticsMS.Api.DTOs
{
    // ── Authentication ──────────────────────────────────────────────────
    public class LoginDto
    {
        [Required] public string Username { get; set; } = string.Empty;
        [Required] public string Password { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }

    // ── Client ──────────────────────────────────────────────────────────
    public class ClientDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ContactDetails { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
    }

    public class ClientCreateDto
    {
        [Required, StringLength(150, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;
        [Required, StringLength(300)]
        public string ContactDetails { get; set; } = string.Empty;
        [Required, StringLength(100)]
        public string Region { get; set; } = string.Empty;
    }

    // ── Contract ────────────────────────────────────────────────────────
    public class ContractDto
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ContractStatus Status { get; set; }
        public ServiceLevel ServiceLevel { get; set; }
        public string? SignedAgreementFileName { get; set; }
    }

    public class ContractCreateDto
    {
        [Required] public int ClientId { get; set; }
        [Required] public DateTime StartDate { get; set; }
        [Required] public DateTime EndDate { get; set; }
        public ContractStatus Status { get; set; } = ContractStatus.Draft;
        public ServiceLevel ServiceLevel { get; set; } = ServiceLevel.Standard;
    }

    /// <summary>Body for PATCH /api/contracts/{id}/status (approve / decline / hold).</summary>
    public class ContractStatusUpdateDto
    {
        [Required] public ContractStatus Status { get; set; }
    }

    // ── Service Request ─────────────────────────────────────────────────
    public class ServiceRequestDto
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        public string Description { get; set; } = string.Empty;
        public RequestType RequestType { get; set; }
        public ServiceRequestStatus Status { get; set; }
        public decimal CostUSD { get; set; }
        public decimal CostZAR { get; set; }
        public decimal ExchangeRateUsed { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class ServiceRequestCreateDto
    {
        [Required] public int ContractId { get; set; }
        [Required, StringLength(500)] public string Description { get; set; } = string.Empty;
        [Required] public RequestType RequestType { get; set; } = RequestType.Domestic;
        [Required, Range(0.01, double.MaxValue, ErrorMessage = "Cost must be greater than zero.")]
        public decimal CostUSD { get; set; }
    }
}
