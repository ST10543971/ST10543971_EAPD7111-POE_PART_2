using Microsoft.EntityFrameworkCore;
using TechMove.Api.Data;
using TechMove.Api.Models;

namespace TechMove.Api.Repositories;

public sealed class ContractRepository(ApplicationDbContext dbContext) : IContractRepository
{
    public async Task<IReadOnlyList<Contract>> SearchAsync(DateTime? startFrom, DateTime? endTo, ContractStatus? status, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Contracts.AsNoTracking().Include(x => x.Client).AsQueryable();
        if (startFrom.HasValue) query = query.Where(x => x.StartDate >= startFrom.Value);
        if (endTo.HasValue) query = query.Where(x => x.EndDate <= endTo.Value);
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        return await query.OrderBy(x => x.StartDate).ToListAsync(cancellationToken);
    }

    public Task<Contract?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.Contracts.Include(x => x.Client).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<Contract> AddAsync(Contract contract, CancellationToken cancellationToken = default)
    {
        dbContext.Contracts.Add(contract);
        await dbContext.SaveChangesAsync(cancellationToken);
        return contract;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => dbContext.SaveChangesAsync(cancellationToken);
}
