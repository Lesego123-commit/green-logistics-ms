// ==========================================================================
//  Green Logistics Management System (GLMS) - Part 3 SOA  |  PROG7311
//  File: GreenLogisticsMS.Api/Services/CurrencyService.cs
//  ----------------------------------------------------------------------
//  References (IIE Harvard):
//   - Microsoft. 2024. Make HTTP requests using IHttpClientFactory. [Online].
//     Available at: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests [Accessed 31 May 2026].
//   - Microsoft. 2024. How to parse JSON with System.Text.Json. [Online]. Available at:
//     https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/ [Accessed 31 May 2026].
//   - Microsoft. 2024. Asynchronous programming with async and await. [Online]. Available at:
//     https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/ [Accessed 31 May 2026].
// ==========================================================================

using System.Text.Json;

namespace GreenLogisticsMS.Api.Services
{
    /// <summary>
    /// Provides USD->ZAR conversion. Calls a live API and gracefully falls back
    /// to a cached rate (18.50) if the external service is unavailable.
    /// </summary>
    public class CurrencyService : ICurrencyService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CurrencyService> _logger;
        private const string ApiUrl = "https://open.er-api.com/v6/latest/USD";
        private const decimal FallbackRate = 18.50m;

        public CurrencyService(IHttpClientFactory httpClientFactory, ILogger<CurrencyService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<decimal> GetUsdToZarRateAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("CurrencyApi");
                var response = await client.GetAsync(ApiUrl);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var rate = doc.RootElement.GetProperty("rates").GetProperty("ZAR").GetDecimal();

                _logger.LogInformation("[CurrencyService] Live USD->ZAR rate: {Rate}", rate);
                return rate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CurrencyService] API unavailable. Using fallback rate {Fallback}.", FallbackRate);
                return FallbackRate;
            }
        }

        public async Task<decimal> ConvertUsdToZarAsync(decimal amountUsd)
        {
            if (amountUsd < 0)
                throw new ArgumentException("Amount cannot be negative.", nameof(amountUsd));

            var rate = await GetUsdToZarRateAsync();
            return Math.Round(amountUsd * rate, 2);
        }
    }
}
