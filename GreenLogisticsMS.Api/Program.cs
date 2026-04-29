// ==========================================================================
//  Green Logistics Management System (GLMS) - Part 3 SOA  |  PROG7311
//  File: GreenLogisticsMS.Api/Program.cs
//  ----------------------------------------------------------------------
//  Composition root for the Web API: EF Core (SQL Server), JWT bearer auth,
//  Swagger/OpenAPI with a bearer scheme, the application services and the
//  design-pattern strategies, plus auto-migration on startup.
//  ----------------------------------------------------------------------
//  References (IIE Harvard):
//   - Microsoft. 2024. Create web APIs with ASP.NET Core. [Online]. Available at:
//     https://learn.microsoft.com/en-us/aspnet/core/web-api/ [Accessed 31 May 2026].
//   - Microsoft. 2024. Configure JWT bearer authentication in ASP.NET Core. [Online].
//     Available at: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt [Accessed 31 May 2026].
//   - Microsoft. 2024. Get started with Swashbuckle and ASP.NET Core. [Online].
//     Available at: https://learn.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle [Accessed 31 May 2026].
//   - Microsoft. 2024. Migrations Overview (EF Core). [Online]. Available at:
//     https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/ [Accessed 31 May 2026].
// ==========================================================================

using System.Text;
using GreenLogisticsMS.Api.Data;
using GreenLogisticsMS.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ── Database (SQL Server) ─────────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Application services ──────────────────────────────────────────────
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddHttpClient("CurrencyApi", c => c.Timeout = TimeSpan.FromSeconds(10));

// ── JWT authentication ────────────────────────────────────────────────
var jwt = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!))
        };
    });
builder.Services.AddAuthorization();

// ── CORS (allow the MVC client) ───────────────────────────────────────
builder.Services.AddCors(options =>
    options.AddPolicy("AllowWeb", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// ── MVC + Swagger ─────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "GLMS Web API", Version = "v1" });

    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter the JWT returned by /api/auth/login (without the 'Bearer ' prefix).",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    c.AddSecurityDefinition("Bearer", scheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { scheme, Array.Empty<string>() } });
});

var app = builder.Build();

// ── Pipeline ──────────────────────────────────────────────────────────
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "GLMS Web API v1"));

app.UseStaticFiles();          // serve uploaded PDFs
app.UseCors("AllowWeb");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "Healthy" })).AllowAnonymous();

// ── Auto-migrate with retry (SQL container may still be starting) ──────
await MigrateWithRetryAsync(app);

app.Run();

static async Task MigrateWithRetryAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var log = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    for (var attempt = 1; attempt <= 10; attempt++)
    {
        try
        {
            await db.Database.MigrateAsync();
            log.LogInformation("[GLMS API] Database migrated successfully.");
            return;
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "[GLMS API] Migration attempt {Attempt}/10 failed. Retrying in 5s...", attempt);
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
    log.LogError("[GLMS API] Could not migrate the database after 10 attempts.");
}

// Exposed so the integration-test WebApplicationFactory can reference the entry point.
public partial class Program { }
