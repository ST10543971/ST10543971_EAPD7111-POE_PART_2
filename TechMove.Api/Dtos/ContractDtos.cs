using System.ComponentModel.DataAnnotations;
using TechMove.Api.Models;

namespace TechMove.Api.Dtos;

public sealed record ContractDto(int Id, int ClientId, string? ClientName, DateTime StartDate, DateTime EndDate, ContractStatus Status, string ServiceLevel, string? SignedAgreementPath);

public sealed class CreateContractDto
{
    [Range(1, int.MaxValue)] public int ClientId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public ContractStatus Status { get; set; } = ContractStatus.Draft;
    [Required, StringLength(200)] public string ServiceLevel { get; set; } = string.Empty;
}

public sealed class UpdateContractStatusDto
{
    public ContractStatus Status { get; set; }
}
