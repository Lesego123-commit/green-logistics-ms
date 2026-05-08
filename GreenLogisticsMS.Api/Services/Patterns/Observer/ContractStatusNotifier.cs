// ==========================================================================
//  Green Logistics Management System (GLMS) - Part 3 SOA  |  PROG7311
//  File: GreenLogisticsMS.Api/Services/Patterns/Observer/ContractStatusNotifier.cs
//  ----------------------------------------------------------------------
//  References (IIE Harvard):
//   - Gamma, E., Helm, R., Johnson, R. & Vlissides, J. 1995. Design Patterns:
//     Elements of Reusable Object-Oriented Software. Boston: Addison-Wesley.
//   - Freeman, A. 2022. Pro ASP.NET Core 6. 9th ed. New York: Apress.
// ==========================================================================

using GreenLogisticsMS.Api.Models;

namespace GreenLogisticsMS.Api.Services.Patterns.Observer
{
    /// <summary>Concrete Subject - notifies observers when a Contract status changes.</summary>
    public class ContractStatusNotifier : IContractSubject
    {
        private readonly List<IContractObserver> _observers = new();

        public void RegisterObserver(IContractObserver observer)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);
        }

        public void RemoveObserver(IContractObserver observer) => _observers.Remove(observer);

        public void NotifyObservers(Contract contract)
        {
            foreach (var observer in _observers)
                observer.Update(contract);
        }
    }
}
