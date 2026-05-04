// ==========================================================================
//  Green Logistics Management System (GLMS) - Part 3 SOA  |  PROG7311
//  File: GreenLogisticsMS.Api/Data/ApplicationDbContext.cs
//  ----------------------------------------------------------------------
//  EF Core DbContext. Configures the relational schema with the Fluent API
//  (keys, constraints, one-to-many relationships) and seeds reference data.
//  ----------------------------------------------------------------------
//  References (IIE Harvard):
//   - Microsoft. 2024. Entity Framework Core. [Online]. Available at:
//     https://learn.microsoft.com/en-us/ef/core/ [Accessed 31 May 2026].
//   - Microsoft. 2024. Creating and configuring a model (Fluent API). [Online].
//     Available at: https://learn.microsoft.com/en-us/ef/core/modeling/ [Accessed 31 May 2026].
//   - Microsoft. 2024. Data seeding (EF Core). [Online]. Available at:
//     https://learn.microsoft.com/en-us/ef/core/modeling/data-seeding [Accessed 31 May 2026].
// ==========================================================================

using GreenLogisticsMS.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GreenLogisticsMS.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Client> Clients => Set<Client>();
        public DbSet<Contract> Contracts => Set<Contract>();
        public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();
        public DbSet<AppUser> Users => Set<AppUser>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Client ──────────────────────────────────────────────────
            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(150);
                entity.Property(c => c.ContactDetails).IsRequired().HasMaxLength(300);
                entity.Property(c => c.Region).IsRequired().HasMaxLength(100);
            });

            // ── Contract ─────────────────────────────────────────────────
            modelBuilder.Entity<Contract>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Status).IsRequired().HasConversion<string>();
                entity.Property(c => c.ServiceLevel).IsRequired().HasConversion<string>();
                entity.Property(c => c.SignedAgreementPath).HasMaxLength(500);
                entity.Property(c => c.SignedAgreementFileName).HasMaxLength(255);

                entity.HasOne(c => c.Client)
                      .WithMany(cl => cl.Contracts)
                      .HasForeignKey(c => c.ClientId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ── ServiceRequest ───────────────────────────────────────────
            modelBuilder.Entity<ServiceRequest>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Description).IsRequired().HasMaxLength(500);
                entity.Property(s => s.RequestType).HasConversion<string>();
                entity.Property(s => s.Status).HasConversion<string>();
                entity.Property(s => s.CostUSD).HasColumnType("decimal(18,2)");
                entity.Property(s => s.CostZAR).HasColumnType("decimal(18,2)");
                entity.Property(s => s.ExchangeRateUsed).HasColumnType("decimal(18,4)");

                entity.HasOne(s => s.Contract)
                      .WithMany(c => c.ServiceRequests)
                      .HasForeignKey(s => s.ContractId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ── AppUser ──────────────────────────────────────────────────
            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.Username).IsUnique();
                entity.Property(u => u.Username).IsRequired().HasMaxLength(100);
                entity.Property(u => u.Role).IsRequired().HasMaxLength(50);
            });

            // ── Seed: default admin user ─────────────────────────────────
            // Password = "Admin@123" (pre-computed SHA256+salt hash, see AuthService).
            modelBuilder.Entity<AppUser>().HasData(new AppUser
            {
                Id = 1,
                Username = "admin",
                // SHA256("GLMS_SALT::Admin@123") -> hex, see AuthService.HashPassword
                PasswordHash = "655b2916510a0cc9cb28117ffd6e1f6516bdf52ada4fcb077a85d683f5ff355e",
                Role = "Admin"
            });

            // ── Seed: sample client ──────────────────────────────────────
            modelBuilder.Entity<Client>().HasData(new Client
            {
                Id = 1,
                Name = "TechMove Logistics",
                ContactDetails = "ops@techmove.example | +27 11 555 0100",
                Region = "Gauteng"
            });
        }
    }
}
