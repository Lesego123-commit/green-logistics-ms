// ==========================================================================
//  Green Logistics Management System (GLMS) - Part 3 SOA  |  PROG7311
//  File: GreenLogisticsMS.Api/Services/Patterns/Strategy/LiveApiStrategy.cs
//  ----------------------------------------------------------------------
//  References (IIE Harvard):
//   - Gamma, E., Helm, R., Johnson, R. & Vlissides, J. 1995. Design Patterns:
//     Elements of Reusable Object-Oriented Software. Boston: Addison-Wesley.
//   - Microsoft. 2024. Make HTTP requests using IHttpClientFactory. [Online].
//     Available at: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests [Accessed 31 May 2026].
//   - Microsoft. 2024. How to parse JSON with System.Text.Json. [Online]. Available at:
//     https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/ [Accessed 31 May 2026].
// ==========================================================================

using System.Text.Json;

namespace GreenLogisticsMS.Api.Services.Patterns.Strategy
{
    /// <summary>Concrete Strategy - fetches a live rate from open.er-api.com (free, no key).</summary>
    public class LiveApiStrategy : IConversionStrategy
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<LiveApiStrategy> _logger;

        public LiveApiStrategy(HttpClient httpClient, ILogger<LiveApiStrategy> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<decimal> ConvertAsync(decimal amount, string from, string to)
        {
            try
            {
                var url = $"https://open.er-api.com/v6/latest/{from.ToUpper()}";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var rate = doc.RootElement.GetProperty("rates").GetProperty(to.ToUpper()).GetDecimal();

                _logger.LogInformation("[LiveApiStrategy] {From}->{To} rate: {Rate}", from, to, rate);
                return Math.Round(amount * rate, 2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[LiveApiStrategy] API call failed.");
                throw new InvalidOperationException("Currency API is currently unavailable.", ex);
            }
        }
    }
}
