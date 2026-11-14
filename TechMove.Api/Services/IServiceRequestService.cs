using TechMove.Api.Models;

namespace TechMove.Api.Services;

public interface IServiceRequestService
{
    Task<IReadOnlyList<ServiceRequest>> GetAsync(int? contractId, CancellationToken cancellationToken = default);
    Task<ServiceRequest> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<ServiceRequest> CreateAsync(ServiceRequest serviceRequest, CancellationToken cancellationToken = default);
}
