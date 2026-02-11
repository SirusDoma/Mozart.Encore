using Microsoft.EntityFrameworkCore;
using Mozart.Data.Contexts;
using Mozart.Data.Entities;

namespace Mozart.Data.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> Find(int id, CancellationToken cancellationToken = default);

    Task<User?> FindByUsername(string username, CancellationToken cancellationToken = default);

    Task Update(User user, CancellationToken cancellation = default);
}

public class UserRepository(IDbContextFactory<MainDbContext> factory)
    : Repository<User, MainDbContext>(factory.CreateDbContext()), IUserRepository
{
    public Task<User?> Find(int id, CancellationToken cancellationToken)
    {
        return DbSet.SingleOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public Task<User?> FindByUsername(string username, CancellationToken cancellationToken)
    {
        return DbSet.SingleOrDefaultAsync(c => c.Username == username, cancellationToken);
    }

    public Task Update(User user, CancellationToken cancellation = default)
    {
        DbSet.Update(user);
        return Task.CompletedTask;
    }
}