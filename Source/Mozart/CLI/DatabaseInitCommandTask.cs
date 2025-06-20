using Microsoft.EntityFrameworkCore;
using Mozart.Data.Contexts;

namespace Mozart.CLI;

public class DatabaseInitCommandTask(UserDbContext context) : ICommandLineTask
{
    public static string Name => "db:migrate";
    public static string Description => "Execute database migration with configured database settings.";

    public async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        await context.Database.MigrateAsync(cancellationToken);
        Console.WriteLine("Database successfully migrated");

        return 0;
    }
} 