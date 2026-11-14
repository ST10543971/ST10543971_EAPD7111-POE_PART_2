using Microsoft.AspNetCore.Http;
using TechMove.Api.Models;

namespace TechMove.Api.Services;

public interface IContractService
{
    Task<IReadOnlyList<Contract>> SearchAsync(DateTime? startFrom, DateTime? endTo, ContractStatus? status, CancellationToken cancellationToken = default);
    Task<Contract> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<Contract> CreateAsync(Contract contract, CancellationToken cancellationToken = default);
    Task<Contract> UpdateStatusAsync(int id, ContractStatus status, CancellationToken cancellationToken = default);
    Task SaveAgreementAsync(int id, IFormFile file, CancellationToken cancellationToken = default);
    Task<(Stream Stream, string FileName)?> OpenAgreementAsync(int id, CancellationToken cancellationToken = default);
}
