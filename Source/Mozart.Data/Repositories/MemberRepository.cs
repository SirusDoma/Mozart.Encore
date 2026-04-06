using Microsoft.EntityFrameworkCore;
using Mozart.Data.Contexts;
using Mozart.Data.Entities;

namespace Mozart.Data.Repositories;

public interface IMemberRepository : IRepository<Member>
{
    public Task<Member?> FindByUsername(string username, CancellationToken cancellationToken = default);
}

public class MemberRepository(IDbContextFactory<MainDbContext> factory)
    : Repository<Member, MainDbContext>(factory.CreateDbContext()), IMemberRepository
{
    public Task<Member?> FindByUsername(string username, CancellationToken cancellationToken)
    {
        return DbSet.AsNoTracking().SingleOrDefaultAsync(c => c.Username == username, cancellationToken);
    }
}
