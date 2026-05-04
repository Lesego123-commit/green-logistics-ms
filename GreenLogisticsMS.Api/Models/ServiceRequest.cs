// ==========================================================================
//  Green Logistics Management System (GLMS) - Part 3 SOA  |  PROG7311
//  File: GreenLogisticsMS.Api/Models/ServiceRequest.cs
//  ----------------------------------------------------------------------
//  References (IIE Harvard):
//   - Microsoft. 2024. System.ComponentModel.DataAnnotations Namespace. [Online].
//     Available at: https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations [Accessed 31 May 2026].
//   - Microsoft. 2024. Entity Framework Core. [Online]. Available at:
//     https://learn.microsoft.com/en-us/ef/core/ [Accessed 31 May 2026].
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenLogisticsMS.Api.Models
{
    public class ServiceRequest
    {
        public int Id { get; set; }

        [Required]
        [ForeignKey("Contract")]
        public int ContractId { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public RequestType RequestType { get; set; } = RequestType.Domestic;

        public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.Pending;

        // Currency fields
        [Column(TypeName = "decimal(18,2)")]
        public decimal CostUSD { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CostZAR { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal ExchangeRateUsed { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation
        public Contract? Contract { get; set; }
    }
}
