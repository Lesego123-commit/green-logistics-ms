// ==========================================================================
//  Green Logistics Management System (GLMS) - Part 3 SOA  |  PROG7311
//  File: GreenLogisticsMS.Api/Services/ICurrencyService.cs
//  ----------------------------------------------------------------------
//  References (IIE Harvard):
//   - Microsoft. 2024. Dependency injection in ASP.NET Core. [Online]. Available at:
//     https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection [Accessed 31 May 2026].
// ==========================================================================

namespace GreenLogisticsMS.Api.Services
{
    public interface ICurrencyService
    {
        Task<decimal> GetUsdToZarRateAsync();
        Task<decimal> ConvertUsdToZarAsync(decimal amountUsd);
    }
}
