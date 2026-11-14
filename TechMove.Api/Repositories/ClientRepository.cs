using Microsoft.EntityFrameworkCore;
using TechMove.Api.Data;
using TechMove.Api.Models;

namespace TechMove.Api.Repositories;

public sealed class ClientRepository(ApplicationDbContext dbContext) : IClientRepository
{
    public async Task<IReadOnlyList<Client>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Clients.AsNoTracking().OrderBy(x => x.Name).ToListAsync(cancellationToken);

    public Task<Client?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.Clients.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<Client> AddAsync(Client client, CancellationToken cancellationToken = default)
    {
        dbContext.Clients.Add(client);
        await dbContext.SaveChangesAsync(cancellationToken);
        return client;
    }
}
