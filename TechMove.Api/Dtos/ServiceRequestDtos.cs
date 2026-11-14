using System.ComponentModel.DataAnnotations;

namespace TechMove.Api.Dtos;

public sealed record ServiceRequestDto(int Id, int ContractId, string Description, decimal AmountUsd, decimal CostZar, decimal ExchangeRate, string Status);

public sealed class CreateServiceRequestDto
{
    [Range(1, int.MaxValue)] public int ContractId { get; set; }
    [Required, StringLength(1000)] public string Description { get; set; } = string.Empty;
    [Range(typeof(decimal), "0", "79228162514264337593543950335")] public decimal AmountUsd { get; set; }
    [StringLength(50)] public string Status { get; set; } = "Pending";
}

public sealed record ExchangeRateDto(decimal Rate, DateTime FetchedAtUtc);
