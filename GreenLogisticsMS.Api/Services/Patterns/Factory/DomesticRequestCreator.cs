// ==========================================================================
//  Green Logistics Management System (GLMS) - Part 3 SOA  |  PROG7311
//  File: GreenLogisticsMS.Api/Services/Patterns/Factory/DomesticRequestCreator.cs
//  ----------------------------------------------------------------------
//  References (IIE Harvard):
//   - Gamma, E., Helm, R., Johnson, R. & Vlissides, J. 1995. Design Patterns:
//     Elements of Reusable Object-Oriented Software. Boston: Addison-Wesley.
//   - Freeman, A. 2022. Pro ASP.NET Core 6. 9th ed. New York: Apress.
// ==========================================================================

using GreenLogisticsMS.Api.Models;

namespace GreenLogisticsMS.Api.Services.Patterns.Factory
{
    /// <summary>Concrete Creator - produces a Domestic ServiceRequest.</summary>
    public class DomesticRequestCreator : IServiceRequestFactory
    {
        public ServiceRequest CreateRequest(int contractId, string description, decimal costUsd, decimal costZar, decimal rateUsed)
        {
            return new ServiceRequest
            {
                ContractId = contractId,
                Description = description,
                RequestType = RequestType.Domestic,
                CostUSD = costUsd,
                CostZAR = costZar,
                ExchangeRateUsed = rateUsed,
                Status = ServiceRequestStatus.Pending,
                CreatedDate = DateTime.UtcNow
            };
        }
    }
}
