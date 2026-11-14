using TechMove.Api.Models;

namespace TechMove.Api.Repositories;

public interface IContractRepository
{
    Task<IReadOnlyList<Contract>> SearchAsync(DateTime? startFrom, DateTime? endTo, ContractStatus? status, CancellationToken cancellationToken = default);
    Task<Contract?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Contract> AddAsync(Contract contract, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
