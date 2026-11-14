using TechMove.Api.Models;
using TechMove.Api.Repositories;

namespace TechMove.Api.Services;

public sealed class ClientService(IClientRepository repository) : IClientService
{
    public Task<IReadOnlyList<Client>> GetAllAsync(CancellationToken cancellationToken = default) => repository.GetAllAsync(cancellationToken);

    public async Task<Client> GetAsync(int id, CancellationToken cancellationToken = default) =>
        await repository.GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException($"Client {id} was not found.");

    public Task<Client> CreateAsync(Client client, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(client.Name)) throw new BusinessRuleException("Client name is required.");
        return repository.AddAsync(client, cancellationToken);
    }
}
