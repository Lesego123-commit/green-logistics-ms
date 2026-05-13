// ==========================================================================
//  Green Logistics Management System (GLMS) - Part 3 SOA  |  PROG7311
//  File: GreenLogisticsMS.Web/Program.cs
//  ----------------------------------------------------------------------
//  MVC presentation layer. Registers a typed HttpClient (ApiClient) pointed
//  at the Web API, plus session state used to hold the JWT after login.
//  ----------------------------------------------------------------------
//  References (IIE Harvard):
//   - Microsoft. 2024. Overview of ASP.NET Core MVC. [Online]. Available at:
//     https://learn.microsoft.com/en-us/aspnet/core/mvc/overview [Accessed 31 May 2026].
//   - Microsoft. 2024. Make HTTP requests using IHttpClientFactory (typed clients).
//     [Online]. Available at: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests [Accessed 31 May 2026].
//   - Microsoft. 2024. Session and state management in ASP.NET Core. [Online].
//     Available at: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state [Accessed 31 May 2026].
// ==========================================================================

using GreenLogisticsMS.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// Session holds the JWT issued by the API.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Typed HttpClient -> Web API. Base address comes from configuration so it
// can be swapped between localhost (dev) and the docker service name (prod).
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5080/";
builder.Services.AddHttpClient<IApiClient, ApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Exposed for the integration-test host (if needed).
public partial class Program { }
