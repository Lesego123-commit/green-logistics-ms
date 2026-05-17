// ==========================================================================
//  Green Logistics Management System (GLMS) - Part 3 SOA  |  PROG7311
//  File: GreenLogisticsMS.Web/Models/ApiModels.cs
//  ----------------------------------------------------------------------
//  Plain C# models that mirror the JSON contract exposed by the Web API.
//  The MVC client (de)serialises these - it has no EF entities of its own.
//  ----------------------------------------------------------------------
//  References (IIE Harvard):
//   - Microsoft. 2024. Make HTTP requests using IHttpClientFactory. [Online].
//     Available at: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests [Accessed 31 May 2026].
//   - Microsoft. 2024. How to serialize and deserialize JSON. [Online]. Available at:
//     https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/ [Accessed 31 May 2026].
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace GreenLogisticsMS.Web.Models
{
    public enum ContractStatus { Draft, Active, Expired, OnHold }
    public enum ServiceLevel { Standard, Priority, Premium }
    public enum ServiceRequestStatus { Pending, InProgress, Completed, Cancelled }
    public enum RequestType { Domestic, International }

    public class LoginModel
    {
        [Required] public string Username { get; set; } = string.Empty;
        [Required, DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }

    public class ClientModel
    {
        public int Id { get; set; }
        [Required, StringLength(150, MinimumLength = 2), Display(Name = "Client Name")]
        public string Name { get; set; } = string.Empty;
        [Required, StringLength(300), Display(Name = "Contact Details")]
        public string ContactDetails { get; set; } = string.Empty;
        [Required, StringLength(100)]
        public string Region { get; set; } = string.Empty;
    }

    public class ContractModel
    {
        public int Id { get; set; }
        [Required, Display(Name = "Client")] public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        [Required, DataType(DataType.Date), Display(Name = "Start Date")] public DateTime StartDate { get; set; } = DateTime.Today;
        [Required, DataType(DataType.Date), Display(Name = "End Date")] public DateTime EndDate { get; set; } = DateTime.Today.AddYears(1);
        public ContractStatus Status { get; set; } = ContractStatus.Draft;
        [Display(Name = "Service Level")] public ServiceLevel ServiceLevel { get; set; } = ServiceLevel.Standard;
        [Display(Name = "Signed Agreement")] public string? SignedAgreementFileName { get; set; }
    }

    public class ContractFilterModel
    {
        public ContractStatus? Status { get; set; }
        public int? ClientId { get; set; }
        [DataType(DataType.Date), Display(Name = "Start From")] public DateTime? StartFrom { get; set; }
        [DataType(DataType.Date), Display(Name = "Start To")] public DateTime? StartTo { get; set; }
        public List<ContractModel> Results { get; set; } = new();
    }

    public class ServiceRequestModel
    {
        public int Id { get; set; }
        [Required, Display(Name = "Contract")] public int ContractId { get; set; }
        [Required, StringLength(500)] public string Description { get; set; } = string.Empty;
        [Required, Display(Name = "Request Type")] public RequestType RequestType { get; set; } = RequestType.Domestic;
        public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.Pending;
        [Required, Range(0.01, double.MaxValue, ErrorMessage = "Cost must be greater than zero.")]
        [Display(Name = "Cost (USD $)")] public decimal CostUSD { get; set; }
        [Display(Name = "Cost (ZAR)")] public decimal CostZAR { get; set; }
        [Display(Name = "Exchange Rate")] public decimal ExchangeRateUsed { get; set; }
        [Display(Name = "Created")] public DateTime CreatedDate { get; set; }
    }

    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
