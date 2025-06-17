using Microsoft.EntityFrameworkCore;
using Mozart.Persistence.Contexts;
using Mozart.Persistence.Entities;

namespace Mozart.Persistence.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> Find(int id, CancellationToken cancellationToken = default);

    Task<User?> FindByUsername(string username, CancellationToken cancellationToken = default);

    Task Update(User user, CancellationToken cancellation = default);
}

public class UserRepository(IDbContextFactory<UserDbContext> factory)
    : Repository<User, UserDbContext>(factory.CreateDbContext()), IUserRepository
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