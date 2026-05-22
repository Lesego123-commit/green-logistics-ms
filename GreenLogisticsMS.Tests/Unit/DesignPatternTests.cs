// ==========================================================================
//  Green Logistics Management System (GLMS) - Part 3 SOA  |  PROG7311
//  File: GreenLogisticsMS.Tests/Unit/DesignPatternTests.cs
//  ----------------------------------------------------------------------
//  Unit tests for the Factory, Observer and Strategy patterns carried over
//  from Part 1/2 and re-homed in the API project.
//  ----------------------------------------------------------------------
//  References (IIE Harvard):
//   - Gamma, E., Helm, R., Johnson, R. & Vlissides, J. 1995. Design Patterns.
//     Boston: Addison-Wesley.
//   - Moq Contributors. 2024. Moq. [Online]. Available at: https://github.com/moq/moq4 [Accessed 31 May 2026].
//   - Microsoft. 2024. xUnit.net. [Online]. Available at: https://xunit.net/ [Accessed 31 May 2026].
// ==========================================================================

using GreenLogisticsMS.Api.Models;
using GreenLogisticsMS.Api.Services.Patterns.Factory;
using GreenLogisticsMS.Api.Services.Patterns.Observer;
using GreenLogisticsMS.Api.Services.Patterns.Strategy;
using Microsoft.Extensions.Logging;
using Moq;

namespace GreenLogisticsMS.Tests.Unit
{
    public class DesignPatternTests
    {
        // ── Factory ──────────────────────────────────────────────────────
        [Fact]
        public void DomesticFactory_ProducesDomesticPendingRequest()
        {
            var sr = new DomesticRequestCreator().CreateRequest(1, "d", 100m, 1850m, 18.5m);
            Assert.Equal(RequestType.Domestic, sr.RequestType);
            Assert.Equal(ServiceRequestStatus.Pending, sr.Status);
        }

        [Fact]
        public void InternationalFactory_ProducesInternationalRequest()
        {
            var sr = new InternationalRequestCreator().CreateRequest(2, "i", 500m, 9250m, 18.5m);
            Assert.Equal(RequestType.International, sr.RequestType);
            Assert.Equal(500m, sr.CostUSD);
        }

        // ── Observer ─────────────────────────────────────────────────────
        [Fact]
        public void Observer_RegisterAndNotify_CallsUpdate()
        {
            var notifier = new ContractStatusNotifier();
            var obs = new Mock<IContractObserver>();
            notifier.RegisterObserver(obs.Object);

            var contract = new Contract { Id = 1, Status = ContractStatus.Expired };
            notifier.NotifyObservers(contract);

            obs.Verify(o => o.Update(contract), Times.Once);
        }

        [Fact]
        public void Observer_RemoveObserver_NotCalled()
        {
            var notifier = new ContractStatusNotifier();
            var obs = new Mock<IContractObserver>();
            notifier.RegisterObserver(obs.Object);
            notifier.RemoveObserver(obs.Object);

            notifier.NotifyObservers(new Contract { Id = 1, Status = ContractStatus.Expired });
            obs.Verify(o => o.Update(It.IsAny<Contract>()), Times.Never);
        }

        // ── Strategy ─────────────────────────────────────────────────────
        [Fact]
        public async Task CachedStrategy_KnownPair_Converts()
        {
            var logger = new Mock<ILogger<CachedRateStrategy>>().Object;
            var strategy = new CachedRateStrategy(logger, new Dictionary<string, decimal> { { "USD_ZAR", 19.00m } });
            Assert.Equal(1900.00m, await strategy.ConvertAsync(100m, "USD", "ZAR"));
        }

        [Fact]
        public async Task Converter_SwitchStrategy_UsesNewStrategy()
        {
            var logger = new Mock<ILogger<CachedRateStrategy>>().Object;
            var s1 = new CachedRateStrategy(logger, new Dictionary<string, decimal> { { "USD_ZAR", 18.00m } });
            var s2 = new CachedRateStrategy(logger, new Dictionary<string, decimal> { { "USD_ZAR", 20.00m } });

            var converter = new CurrencyConverter(s1);
            var r1 = await converter.ConvertAsync(100m);
            converter.SetStrategy(s2);
            var r2 = await converter.ConvertAsync(100m);

            Assert.Equal(1800.00m, r1);
            Assert.Equal(2000.00m, r2);
        }
    }
}
