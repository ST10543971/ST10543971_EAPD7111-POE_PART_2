using System.ComponentModel.DataAnnotations;

namespace TechMove.Api.Dtos;

public sealed record ClientDto(int Id, string Name, string ContactDetails, string Region);

public sealed class CreateClientDto
{
    [Required, StringLength(200)] public string Name { get; set; } = string.Empty;
    [Required, StringLength(500)] public string ContactDetails { get; set; } = string.Empty;
    [Required, StringLength(100)] public string Region { get; set; } = string.Empty;
}
