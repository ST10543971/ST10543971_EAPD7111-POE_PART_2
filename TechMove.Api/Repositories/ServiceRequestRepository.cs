using Microsoft.EntityFrameworkCore;
using TechMove.Api.Data;
using TechMove.Api.Models;

namespace TechMove.Api.Repositories;

public sealed class ServiceRequestRepository(ApplicationDbContext dbContext) : IServiceRequestRepository
{
    public async Task<IReadOnlyList<ServiceRequest>> GetAsync(int? contractId, CancellationToken cancellationToken = default)
    {
        var query = dbContext.ServiceRequests.AsNoTracking().Include(x => x.Contract).AsQueryable();
        if (contractId.HasValue) query = query.Where(x => x.ContractId == contractId.Value);
        return await query.OrderByDescending(x => x.Id).ToListAsync(cancellationToken);
    }

    public Task<ServiceRequest?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.ServiceRequests.Include(x => x.Contract).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<ServiceRequest> AddAsync(ServiceRequest serviceRequest, CancellationToken cancellationToken = default)
    {
        dbContext.ServiceRequests.Add(serviceRequest);
        await dbContext.SaveChangesAsync(cancellationToken);
        return serviceRequest;
    }
}
