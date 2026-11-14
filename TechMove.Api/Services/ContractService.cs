using Microsoft.AspNetCore.Http;
using TechMove.Api.Models;
using TechMove.Api.Repositories;

namespace TechMove.Api.Services;

public sealed class ContractService(IContractRepository repository, IFileStorageService fileStorage) : IContractService
{
    private const long MaxFileSize = 10 * 1024 * 1024;

    public Task<IReadOnlyList<Contract>> SearchAsync(DateTime? startFrom, DateTime? endTo, ContractStatus? status, CancellationToken cancellationToken = default) =>
        repository.SearchAsync(startFrom, endTo, status, cancellationToken);

    public async Task<Contract> GetAsync(int id, CancellationToken cancellationToken = default) =>
        await repository.GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException($"Contract {id} was not found.");

    public async Task<Contract> CreateAsync(Contract contract, CancellationToken cancellationToken = default)
    {
        if (contract.EndDate < contract.StartDate) throw new BusinessRuleException("The contract end date must be on or after the start date.");
        return await repository.AddAsync(contract, cancellationToken);
    }

    public async Task<Contract> UpdateStatusAsync(int id, ContractStatus status, CancellationToken cancellationToken = default)
    {
        var contract = await GetAsync(id, cancellationToken);
        var validTransition = contract.Status switch
        {
            ContractStatus.Draft => status is ContractStatus.Active or ContractStatus.OnHold or ContractStatus.Expired,
            ContractStatus.Active => status is ContractStatus.OnHold or ContractStatus.Expired,
            ContractStatus.OnHold => status is ContractStatus.Active or ContractStatus.Expired,
            ContractStatus.Expired => false,
            _ => false
        };
        if (!validTransition) throw new BusinessRuleException($"The contract cannot transition from {contract.Status} to {status}.");
        contract.Status = status;
        await repository.SaveChangesAsync(cancellationToken);
        return contract;
    }

    public async Task SaveAgreementAsync(int id, IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0) throw new BusinessRuleException("A PDF agreement is required.");
        if (file.Length > MaxFileSize) throw new BusinessRuleException("The agreement must not exceed 10 MB.");
        if (!string.Equals(Path.GetExtension(file.FileName), ".pdf", StringComparison.OrdinalIgnoreCase)) throw new BusinessRuleException("Only PDF agreements are accepted.");

        var contract = await GetAsync(id, cancellationToken);
        await using var stream = file.OpenReadStream();
        contract.SignedAgreementPath = await fileStorage.SavePdfAsync(stream, ".pdf", cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<(Stream Stream, string FileName)?> OpenAgreementAsync(int id, CancellationToken cancellationToken = default)
    {
        var contract = await GetAsync(id, cancellationToken);
        if (string.IsNullOrWhiteSpace(contract.SignedAgreementPath)) return null;
        var stream = await fileStorage.OpenReadAsync(contract.SignedAgreementPath, cancellationToken);
        return stream is null ? null : (stream, contract.SignedAgreementPath);
    }
}
