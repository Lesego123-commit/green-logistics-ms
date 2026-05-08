// ==========================================================================
//  Green Logistics Management System (GLMS) - Part 3 SOA  |  PROG7311
//  File: GreenLogisticsMS.Api/Controllers/AuthController.cs
//  ----------------------------------------------------------------------
//  Issues JWT bearer tokens. POST /api/auth/login returns a token that the
//  MVC client (and Swagger) attach as 'Authorization: Bearer <token>'.
//  ----------------------------------------------------------------------
//  References (IIE Harvard):
//   - Microsoft. 2024. Create web APIs with ASP.NET Core. [Online]. Available at:
//     https://learn.microsoft.com/en-us/aspnet/core/web-api/ [Accessed 31 May 2026].
//   - Microsoft. 2024. Authentication and authorization in ASP.NET Core. [Online].
//     Available at: https://learn.microsoft.com/en-us/aspnet/core/security/ [Accessed 31 May 2026].
// ==========================================================================

using GreenLogisticsMS.Api.DTOs;
using GreenLogisticsMS.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenLogisticsMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService) => _authService = authService;

        /// <summary>Authenticates a user and returns a JWT.</summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            if (result == null)
                return Unauthorized(new { message = "Invalid username or password." });
            return Ok(result);
        }
    }
}
