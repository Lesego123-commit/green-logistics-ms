// ==========================================================================
//  Green Logistics Management System (GLMS) - Part 3 SOA  |  PROG7311
//  File: GreenLogisticsMS.Api/Models/AppUser.cs
//  ----------------------------------------------------------------------
//  References (IIE Harvard):
//   - Microsoft. 2024. Authentication and authorization in ASP.NET Core. [Online].
//     Available at: https://learn.microsoft.com/en-us/aspnet/core/security/ [Accessed 31 May 2026].
//   - Microsoft. 2024. Hash passwords in ASP.NET Core. [Online]. Available at:
//     https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/consumer-apis/password-hashing [Accessed 31 May 2026].
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace GreenLogisticsMS.Api.Models
{
    /// <summary>
    /// Application user used for JWT authentication. The password is never stored
    /// in plain text - only a salted hash is persisted to the database.
    /// </summary>
    public class AppUser
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = "Staff";
    }
}
