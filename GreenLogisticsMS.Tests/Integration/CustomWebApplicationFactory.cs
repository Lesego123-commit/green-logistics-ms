// ==========================================================================
//  Green Logistics Management System (GLMS) - Part 3 SOA  |  PROG7311
//  File: GreenLogisticsMS.Tests/Integration/CustomWebApplicationFactory.cs
//  ----------------------------------------------------------------------
//  Boots the real Web API in-memory for integration testing. The SQL Server
//  DbContext is swapped for an EF Core in-memory database and a known test
//  user is seeded, so the tests can run anywhere (including CI) without a
//  live database. This is the host the integration tests call against.
//  ----------------------------------------------------------------------
//  References (IIE Harvard):
//   - Microsoft. 2024. Integration tests in ASP.NET Core. [Online]. Available at:
//     https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests [Accessed 31 May 2026].
//   - Microsoft. 2024. WebApplicationFactory<TEntryPoint> Class. [Online]. Available at:
//     https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.testing.webapplicationfactory-1 [Accessed 31 May 2026].
//   - Microsoft. 2024. Testing with the EF Core In-Memory database provider. [Online].
//     Available at: https://learn.microsoft.com/en-us/ef/core/testing/testing-without-the-database [Accessed 31 May 2026].
// ==========================================================================

using GreenLogisticsMS.Api.Data;
using GreenLogisticsMS.Api.Models;
using GreenLogisticsMS.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GreenLogisticsMS.Tests.Integration
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Remove the SQL Server DbContext registration.
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                // Register an in-memory database instead.
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase("GLMS_IntegrationTests"));

                // Build the provider and seed deterministic test data.
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.EnsureCreated();
                SeedTestData(db);
            });
        }

        private static void SeedTestData(ApplicationDbContext db)
        {
            if (db.Users.Any()) return;

            db.Users.Add(new AppUser
            {
                Id = 1,
                Username = "admin",
                PasswordHash = AuthService.HashPassword("Admin@123"),
                Role = "Admin"
            });

            db.Clients.Add(new Client
            {
                Id = 1,
                Name = "TechMove Logistics",
                ContactDetails = "ops@techmove.example",
                Region = "Gauteng"
            });

            db.Contracts.Add(new Contract
            {
                Id = 1,
                ClientId = 1,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 12, 31),
                Status = ContractStatus.Active,
                ServiceLevel = ServiceLevel.Standard
            });

            db.SaveChanges();
        }
    }
}
