using TechMove.Api.Models;

namespace TechMove.Api.Repositories;

public interface IClientRepository
{
    Task<IReadOnlyList<Client>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Client?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Client> AddAsync(Client client, CancellationToken cancellationToken = default);
}
