// ==========================================================================
//  Green Logistics Management System (GLMS) - Part 3 SOA  |  PROG7311
//  File: GreenLogisticsMS.Api/Services/IAuthService.cs
//  ----------------------------------------------------------------------
//  References (IIE Harvard):
//   - Microsoft. 2024. Authentication and authorization in ASP.NET Core. [Online].
//     Available at: https://learn.microsoft.com/en-us/aspnet/core/security/ [Accessed 31 May 2026].
// ==========================================================================

using GreenLogisticsMS.Api.DTOs;

namespace GreenLogisticsMS.Api.Services
{
    public interface IAuthService
    {
        /// <summary>Validates credentials and returns a JWT, or null if invalid.</summary>
        Task<AuthResponseDto?> LoginAsync(LoginDto dto);
    }
}
