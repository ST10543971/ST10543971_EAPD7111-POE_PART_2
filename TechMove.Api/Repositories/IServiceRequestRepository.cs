using TechMove.Api.Models;

namespace TechMove.Api.Repositories;

public interface IServiceRequestRepository
{
    Task<IReadOnlyList<ServiceRequest>> GetAsync(int? contractId, CancellationToken cancellationToken = default);
    Task<ServiceRequest?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ServiceRequest> AddAsync(ServiceRequest serviceRequest, CancellationToken cancellationToken = default);
}
