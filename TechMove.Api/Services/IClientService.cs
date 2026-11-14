using TechMove.Api.Models;

namespace TechMove.Api.Services;

public interface IClientService
{
    Task<IReadOnlyList<Client>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Client> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<Client> CreateAsync(Client client, CancellationToken cancellationToken = default);
}
