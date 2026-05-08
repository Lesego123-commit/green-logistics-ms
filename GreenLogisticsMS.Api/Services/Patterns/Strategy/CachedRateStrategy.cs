// ==========================================================================
//  Green Logistics Management System (GLMS) - Part 3 SOA  |  PROG7311
//  File: GreenLogisticsMS.Api/Services/Patterns/Strategy/CachedRateStrategy.cs
//  ----------------------------------------------------------------------
//  References (IIE Harvard):
//   - Gamma, E., Helm, R., Johnson, R. & Vlissides, J. 1995. Design Patterns:
//     Elements of Reusable Object-Oriented Software. Boston: Addison-Wesley.
//   - Microsoft. 2024. Logging in C# and .NET. [Online]. Available at:
//     https://learn.microsoft.com/en-us/dotnet/core/extensions/logging [Accessed 31 May 2026].
// ==========================================================================

namespace GreenLogisticsMS.Api.Services.Patterns.Strategy
{
    /// <summary>Concrete Strategy - uses an in-memory rate cache (fallback when API is down).</summary>
    public class CachedRateStrategy : IConversionStrategy
    {
        private readonly Dictionary<string, decimal> _rateCache;
        private readonly ILogger<CachedRateStrategy> _logger;

        public CachedRateStrategy(ILogger<CachedRateStrategy> logger, Dictionary<string, decimal>? seedRates = null)
        {
            _logger = logger;
            _rateCache = seedRates ?? new Dictionary<string, decimal>
            {
                { "USD_ZAR", 18.50m },
                { "EUR_ZAR", 20.10m },
                { "GBP_ZAR", 23.40m }
            };
        }

        public Task<decimal> ConvertAsync(decimal amount, string from, string to)
        {
            var key = $"{from.ToUpper()}_{to.ToUpper()}";
            if (_rateCache.TryGetValue(key, out var rate))
            {
                _logger.LogInformation("[CachedRateStrategy] Using cached rate {Key}: {Rate}", key, rate);
                return Task.FromResult(Math.Round(amount * rate, 2));
            }
            _logger.LogWarning("[CachedRateStrategy] No cached rate for {Key}.", key);
            return Task.FromResult(0m);
        }

        public void UpdateCache(string from, string to, decimal rate)
            => _rateCache[$"{from.ToUpper()}_{to.ToUpper()}"] = rate;
    }
}
