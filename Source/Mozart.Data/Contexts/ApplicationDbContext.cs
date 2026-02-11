using Microsoft.EntityFrameworkCore;

namespace Mozart.Data.Contexts;

public interface IApplicationDbContext;

public abstract class ApplicationDbContext(DbContextOptions contextOptions)
    : DbContext(contextOptions), IApplicationDbContext
{
}
