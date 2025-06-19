using Mozart.Persistence.Contexts;

namespace Mozart.CLI;

public class DatabaseInitCommandTask(UserDbContext context) : ICommandLineTask
{
    public static string Name => "db:init";
    public static string Description => "Execute database initialization with configured database settings.";

    public async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        await context.Database.EnsureCreatedAsync(cancellationToken);
        Console.WriteLine("Database successfully created");

        return 0;
    }
} 