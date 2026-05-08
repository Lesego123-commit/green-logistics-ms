// ==========================================================================
//  Green Logistics Management System (GLMS) - Part 3 SOA  |  PROG7311
//  File: GreenLogisticsMS.Api/Services/Patterns/Strategy/IConversionStrategy.cs
//  ----------------------------------------------------------------------
//  References (IIE Harvard):
//   - Gamma, E., Helm, R., Johnson, R. & Vlissides, J. 1995. Design Patterns:
//     Elements of Reusable Object-Oriented Software. Boston: Addison-Wesley.
//   - Freeman, A. 2022. Pro ASP.NET Core 6. 9th ed. New York: Apress.
// ==========================================================================

namespace GreenLogisticsMS.Api.Services.Patterns.Strategy
{
    /// <summary>Strategy interface for currency conversion.</summary>
    public interface IConversionStrategy
    {
        Task<decimal> ConvertAsync(decimal amount, string from, string to);
    }
}
