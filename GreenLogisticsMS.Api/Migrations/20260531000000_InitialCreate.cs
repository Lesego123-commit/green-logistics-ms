// ==========================================================================
//  Green Logistics Management System (GLMS) - Part 3 SOA  |  PROG7311
//  File: GreenLogisticsMS.Api/Migrations/20260531000000_InitialCreate.cs
//  ----------------------------------------------------------------------
//  EF Core migration that creates the relational schema (Clients, Contracts,
//  ServiceRequests, Users) with keys, constraints and the seed data. This
//  addresses the Part 2 feedback ("No migration script provided").
//  ----------------------------------------------------------------------
//  References (IIE Harvard):
//   - Microsoft. 2024. Migrations Overview (EF Core). [Online]. Available at:
//     https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/ [Accessed 31 May 2026].
// ==========================================================================

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenLogisticsMS.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ContactDetails = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Region = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_Clients", x => x.Id));

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_Users", x => x.Id));

            migrationBuilder.CreateTable(
                name: "Contracts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ServiceLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SignedAgreementPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SignedAgreementFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contracts_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RequestType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CostUSD = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CostZAR = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExchangeRateUsed = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceRequests_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // ── Seed data ────────────────────────────────────────────────
            migrationBuilder.InsertData(
                table: "Clients",
                columns: new[] { "Id", "Name", "ContactDetails", "Region" },
                values: new object[] { 1, "TechMove Logistics", "ops@techmove.example | +27 11 555 0100", "Gauteng" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Username", "PasswordHash", "Role" },
                values: new object[] { 1, "admin", "655b2916510a0cc9cb28117ffd6e1f6516bdf52ada4fcb077a85d683f5ff355e", "Admin" });

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ClientId",
                table: "Contracts",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_ContractId",
                table: "ServiceRequests",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ServiceRequests");
            migrationBuilder.DropTable(name: "Users");
            migrationBuilder.DropTable(name: "Contracts");
            migrationBuilder.DropTable(name: "Clients");
        }
    }
}
