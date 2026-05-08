// ==========================================================================
//  Green Logistics Management System (GLMS) - Part 3 SOA  |  PROG7311
//  File: GreenLogisticsMS.Api/Services/AuthService.cs
//  ----------------------------------------------------------------------
//  Validates user credentials against the database (passwords are stored as
//  salted SHA-256 hashes, never in plain text) and issues a signed JWT.
//  ----------------------------------------------------------------------
//  References (IIE Harvard):
//   - Microsoft. 2024. Authentication and authorization in ASP.NET Core. [Online].
//     Available at: https://learn.microsoft.com/en-us/aspnet/core/security/ [Accessed 31 May 2026].
//   - Microsoft. 2024. JwtSecurityTokenHandler / System.IdentityModel.Tokens.Jwt. [Online].
//     Available at: https://learn.microsoft.com/en-us/dotnet/api/system.identitymodel.tokens.jwt [Accessed 31 May 2026].
//   - Microsoft. 2024. SHA256 Class. [Online]. Available at:
//     https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.sha256 [Accessed 31 May 2026].
// ==========================================================================

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using GreenLogisticsMS.Api.Data;
using GreenLogisticsMS.Api.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace GreenLogisticsMS.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthService> _logger;
        private const string Salt = "GLMS_SALT::";

        public AuthService(ApplicationDbContext context, IConfiguration config, ILogger<AuthService> logger)
        {
            _context = context;
            _config = config;
            _logger = logger;
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == dto.Username);

            if (user == null || user.PasswordHash != HashPassword(dto.Password))
            {
                _logger.LogWarning("[AuthService] Failed login attempt for '{Username}'.", dto.Username);
                return null;
            }

            var jwt = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddHours(double.Parse(jwt["ExpiryHours"] ?? "8"));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            _logger.LogInformation("[AuthService] Issued JWT for '{Username}'.", user.Username);

            return new AuthResponseDto
            {
                Token = tokenString,
                Username = user.Username,
                Role = user.Role,
                ExpiresAt = expires
            };
        }

        /// <summary>Salted SHA-256 hash (hex, lowercase). Matches the seeded admin hash.</summary>
        public static string HashPassword(string password)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(Salt + password));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }
}
