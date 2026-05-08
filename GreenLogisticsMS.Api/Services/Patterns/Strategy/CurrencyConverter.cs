// ==========================================================================
//  Green Logistics Management System (GLMS) - Part 3 SOA  |  PROG7311
//  File: GreenLogisticsMS.Api/Services/Patterns/Strategy/CurrencyConverter.cs
//  ----------------------------------------------------------------------
//  References (IIE Harvard):
//   - Gamma, E., Helm, R., Johnson, R. & Vlissides, J. 1995. Design Patterns:
//     Elements of Reusable Object-Oriented Software. Boston: Addison-Wesley.
//   - Freeman, A. 2022. Pro ASP.NET Core 6. 9th ed. New York: Apress.
// ==========================================================================

namespace GreenLogisticsMS.Api.Services.Patterns.Strategy
{
    /// <summary>Context class for the Strategy pattern - delegates to an IConversionStrategy.</summary>
    public class CurrencyConverter
    {
        private IConversionStrategy _strategy;

        public CurrencyConverter(IConversionStrategy strategy) => _strategy = strategy;

        public void SetStrategy(IConversionStrategy strategy) => _strategy = strategy;

        public async Task<decimal> ConvertAsync(decimal amount, string from = "USD", string to = "ZAR")
            => await _strategy.ConvertAsync(amount, from, to);
    }
}
