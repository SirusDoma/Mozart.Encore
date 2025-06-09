using Microsoft.EntityFrameworkCore;

namespace Mozart.Persistence.Contexts;

public interface IApplicationDbContext;

public abstract class ApplicationDbContext(DbContextOptions contextOptions)
    : DbContext(contextOptions), IApplicationDbContext
{
}