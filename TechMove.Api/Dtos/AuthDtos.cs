using System.ComponentModel.DataAnnotations;

namespace TechMove.Api.Dtos;

public sealed class LoginRequestDto
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
}

public sealed record LoginResponseDto(string Token, DateTime ExpiresAtUtc, string Role, string Email);
