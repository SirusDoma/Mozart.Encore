using Microsoft.EntityFrameworkCore;

namespace Mozart.Data.Repositories;

public interface IRepository<T>
    where T : class
{
    IQueryable<T> AsQueryable();

    Task<int> Commit(CancellationToken cancellationToken = default);
}

public abstract class Repository<T, TDbContext>(TDbContext context) : IRepository<T>
    where T : class
    where TDbContext : DbContext
{
    protected DbSet<T> DbSet { get; } = context.Set<T>();

    protected TDbContext Context => context;

    public IQueryable<T> AsQueryable()
        => DbSet.AsQueryable();

    public Task<int> Commit(CancellationToken cancellationToken)
    {
        return Context.SaveChangesAsync(cancellationToken);
    }
}