// ==========================================================================
//  Green Logistics Management System (GLMS) - Part 3 SOA  |  PROG7311
//  File: GreenLogisticsMS.Api/Services/Patterns/Observer/ExpiryAlertService.cs
//  ----------------------------------------------------------------------
//  References (IIE Harvard):
//   - Gamma, E., Helm, R., Johnson, R. & Vlissides, J. 1995. Design Patterns:
//     Elements of Reusable Object-Oriented Software. Boston: Addison-Wesley.
//   - Microsoft. 2024. Logging in C# and .NET. [Online]. Available at:
//     https://learn.microsoft.com/en-us/dotnet/core/extensions/logging [Accessed 31 May 2026].
// ==========================================================================

using GreenLogisticsMS.Api.Models;

namespace GreenLogisticsMS.Api.Services.Patterns.Observer
{
    /// <summary>Concrete Observer - logs an alert when a Contract becomes Expired or OnHold.</summary>
    public class ExpiryAlertService : IContractObserver
    {
        private readonly ILogger<ExpiryAlertService> _logger;

        public ExpiryAlertService(ILogger<ExpiryAlertService> logger) => _logger = logger;

        public void Update(Contract contract)
        {
            if (contract.Status == ContractStatus.Expired)
            {
                _logger.LogWarning(
                    "[ExpiryAlertService] Contract #{ContractId} for Client #{ClientId} has EXPIRED on {Date}.",
                    contract.Id, contract.ClientId, DateTime.UtcNow.ToShortDateString());
            }
            else if (contract.Status == ContractStatus.OnHold)
            {
                _logger.LogWarning(
                    "[ExpiryAlertService] Contract #{ContractId} for Client #{ClientId} placed ON HOLD on {Date}.",
                    contract.Id, contract.ClientId, DateTime.UtcNow.ToShortDateString());
            }
        }

        public virtual void SendAlert(Contract contract) => Update(contract);
    }
}
