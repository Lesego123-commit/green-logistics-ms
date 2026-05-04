// ==========================================================================
//  Green Logistics Management System (GLMS) - Part 3 SOA  |  PROG7311
//  File: GreenLogisticsMS.Api/Models/Contract.cs
//  ----------------------------------------------------------------------
//  References (IIE Harvard):
//   - Microsoft. 2024. System.ComponentModel.DataAnnotations Namespace. [Online].
//     Available at: https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations [Accessed 31 May 2026].
//   - Microsoft. 2024. Creating and configuring a model (EF Core Fluent API). [Online].
//     Available at: https://learn.microsoft.com/en-us/ef/core/modeling/ [Accessed 31 May 2026].
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenLogisticsMS.Api.Models
{
    public class Contract
    {
        public int Id { get; set; }

        [Required]
        [ForeignKey("Client")]
        public int ClientId { get; set; }

        [Required(ErrorMessage = "Start date is required.")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required.")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required]
        public ContractStatus Status { get; set; } = ContractStatus.Draft;

        [Required]
        public ServiceLevel ServiceLevel { get; set; } = ServiceLevel.Standard;

        // PDF signed agreement (path stored in DB, file on disk)
        public string? SignedAgreementPath { get; set; }
        public string? SignedAgreementFileName { get; set; }

        // Navigation
        public Client? Client { get; set; }
        public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
    }
}
