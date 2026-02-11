using Microsoft.EntityFrameworkCore;

namespace Mozart.Contexts;

public interface IContext
{
    Task<int> Commit();
}

public class StaticDbContextFactory<TContext>(IDbContextFactory<TContext> factory): IDbContextFactory<TContext>
    where TContext : DbContext
{
    private readonly TContext _context = factory.CreateDbContext();

    public TContext CreateDbContext()
    {
        return _context;
    }
}
