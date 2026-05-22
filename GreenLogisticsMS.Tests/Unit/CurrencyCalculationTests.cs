// ==========================================================================
//  Green Logistics Management System (GLMS) - Part 3 SOA  |  PROG7311
//  File: GreenLogisticsMS.Tests/Unit/CurrencyCalculationTests.cs
//  ----------------------------------------------------------------------
//  Unit tests for USD->ZAR conversion (happy path, edge cases, API-down
//  fallback, parameterised precision). Uses a mocked HttpMessageHandler so
//  no real network call is made.
//  ----------------------------------------------------------------------
//  References (IIE Harvard):
//   - Freeman, S. & Pryce, N. 2009. Growing Object-Oriented Software, Guided by Tests.
//     Upper Saddle River: Addison-Wesley.
//   - Microsoft. 2024. Unit testing in .NET. [Online]. Available at:
//     https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test [Accessed 31 May 2026].
//   - Moq Contributors. 2024. Moq. [Online]. Available at: https://github.com/moq/moq4 [Accessed 31 May 2026].
// ==========================================================================

using System.Net;
using System.Text;
using GreenLogisticsMS.Api.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace GreenLogisticsMS.Tests.Unit
{
    public class CurrencyCalculationTests
    {
        private static CurrencyService BuildService(string json, HttpStatusCode code = HttpStatusCode.OK)
        {
            var handler = new MockHttpMessageHandler(json, code);
            var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://open.er-api.com/") };

            var factory = new Mock<IHttpClientFactory>();
            factory.Setup(f => f.CreateClient("CurrencyApi")).Returns(httpClient);

            return new CurrencyService(factory.Object, new Mock<ILogger<CurrencyService>>().Object);
        }

        private static string Json(decimal zar) => $"{{\"result\":\"success\",\"rates\":{{\"ZAR\":{zar},\"USD\":1}}}}";

        [Fact]
        public async Task Convert_StandardAmount_ReturnsCorrectValue()
        {
            var service = BuildService(Json(18.50m));
            Assert.Equal(1850.00m, await service.ConvertUsdToZarAsync(100m));
        }

        [Fact]
        public async Task Convert_ZeroAmount_ReturnsZero()
        {
            var service = BuildService(Json(18.50m));
            Assert.Equal(0m, await service.ConvertUsdToZarAsync(0m));
        }

        [Fact]
        public async Task Convert_NegativeAmount_Throws()
        {
            var service = BuildService(Json(18.50m));
            await Assert.ThrowsAsync<ArgumentException>(() => service.ConvertUsdToZarAsync(-5m));
        }

        [Fact]
        public async Task GetRate_ApiDown_ReturnsFallback()
        {
            var service = BuildService("error", HttpStatusCode.InternalServerError);
            Assert.Equal(18.50m, await service.GetUsdToZarRateAsync());
        }

        [Theory]
        [InlineData(100, 18.50, 1850.00)]
        [InlineData(200, 18.50, 3700.00)]
        [InlineData(1, 20.00, 20.00)]
        [InlineData(500, 19.25, 9625.00)]
        public async Task Convert_Parameterised_MatchesExpected(double usd, double rate, double expected)
        {
            var service = BuildService(Json((decimal)rate));
            Assert.Equal((decimal)expected, await service.ConvertUsdToZarAsync((decimal)usd));
        }
    }

    internal class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _content;
        private readonly HttpStatusCode _code;
        public MockHttpMessageHandler(string content, HttpStatusCode code) { _content = content; _code = code; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
            => Task.FromResult(new HttpResponseMessage(_code)
            {
                Content = new StringContent(_content, Encoding.UTF8, "application/json")
            });
    }
}
