// ==========================================================================
//  Green Logistics Management System (GLMS) - Part 3 SOA  |  PROG7311
//  File: GreenLogisticsMS.Api/Services/Patterns/Observer/IContractObserver.cs
//  ----------------------------------------------------------------------
//  References (IIE Harvard):
//   - Gamma, E., Helm, R., Johnson, R. & Vlissides, J. 1995. Design Patterns:
//     Elements of Reusable Object-Oriented Software. Boston: Addison-Wesley.
//   - Freeman, A. 2022. Pro ASP.NET Core 6. 9th ed. New York: Apress.
// ==========================================================================

using GreenLogisticsMS.Api.Models;

namespace GreenLogisticsMS.Api.Services.Patterns.Observer
{
    /// <summary>Observer - reacts when a Contract's status changes.</summary>
    public interface IContractObserver
    {
        void Update(Contract contract);
    }
}
