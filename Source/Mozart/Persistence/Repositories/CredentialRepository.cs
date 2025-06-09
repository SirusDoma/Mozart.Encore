using Microsoft.EntityFrameworkCore;
using Mozart.Persistence.Contexts;
using Mozart.Persistence.Entities;

namespace Mozart.Persistence.Repositories;

public interface ICredentialRepository : IRepository<Credential>
{
    public Task<Credential?> FindByUsername(string username, CancellationToken cancellationToken = default);
}

public class CredentialRepository(IDbContextFactory<UserDbContext> factory)
    : Repository<Credential, UserDbContext>(factory.CreateDbContext()), ICredentialRepository
{
    public Task<Credential?> FindByUsername(string username, CancellationToken cancellationToken)
    {
        return DbSet.AsNoTracking().SingleOrDefaultAsync(c => c.Username == username, cancellationToken);
    }
}