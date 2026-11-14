using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TechMove.Api.Data;
using TechMove.Api.Dtos;
using TechMove.Api.Models;

namespace TechMove.Api.Services;

public sealed class AuthService(ApplicationDbContext dbContext, IConfiguration configuration) : IAuthService
{
    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.AppUsers.SingleOrDefaultAsync(x => x.Email == request.Email, cancellationToken);
        if (user is null) return null;
        var result = new PasswordHasher<AppUser>().VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed) return null;

        var expiresAt = DateTime.UtcNow.AddMinutes(60);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };
        var key = configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key must be configured.");
        var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(configuration["Jwt:Issuer"], configuration["Jwt:Audience"], claims, expires: expiresAt, signingCredentials: signingCredentials);
        return new LoginResponseDto(new JwtSecurityTokenHandler().WriteToken(token), expiresAt, user.Role, user.Email);
    }
}
