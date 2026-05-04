// ==========================================================================
//  Green Logistics Management System (GLMS) - Part 3 SOA  |  PROG7311
//  File: GreenLogisticsMS.Api/Models/Client.cs
//  ----------------------------------------------------------------------
//  References (IIE Harvard):
//   - Microsoft. 2024. System.ComponentModel.DataAnnotations Namespace. [Online].
//     Available at: https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations [Accessed 31 May 2026].
//   - Microsoft. 2024. Entity Framework Core. [Online]. Available at:
//     https://learn.microsoft.com/en-us/ef/core/ [Accessed 31 May 2026].
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace GreenLogisticsMS.Api.Models
{
    public class Client
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Client name is required.")]
        [StringLength(150, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact details are required.")]
        [StringLength(300)]
        public string ContactDetails { get; set; } = string.Empty;

        [Required(ErrorMessage = "Region is required.")]
        [StringLength(100)]
        public string Region { get; set; } = string.Empty;

        // Navigation
        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    }
}
