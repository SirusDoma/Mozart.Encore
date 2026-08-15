using Encore.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Mozart.Data.Contexts;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<MainDbContext>
{
    public MainDbContext CreateDbContext(string[] args)
    {
        string driver = args
            .Select(a => a.Split('='))
            .Where(a => a.Length == 2 && a[0].Equals("Db:Driver", StringComparison.OrdinalIgnoreCase))
            .Select(a => a[1])
            .LastOrDefault() ?? "Sqlite";

        var builder = new DbContextOptionsBuilder<MainDbContext>();
        _ = driver switch
        {
            "Sqlite"    => builder.UseSqlite("Data Source=O2JAM.db",
                ctx => ctx.MigrationsAssembly("Memoryer.Migrations.Sqlite")),
            "SqlServer" => builder.UseSqlServer("Server=localhost;Database=O2JAM;Trusted_Connection=True;TrustServerCertificate=True",
                ctx => ctx.MigrationsAssembly("Memoryer.Migrations.SqlServer")),
            "MySql"     => builder.UseMySQL("Server=localhost;Database=O2JAM;Uid=root;Pwd=;",
                ctx => ctx.MigrationsAssembly("Memoryer.Migrations.MySql")),
            "Postgres"  => builder.UseNpgsql("Host=localhost;Database=O2JAM;Username=postgres",
                ctx => ctx.MigrationsAssembly("Memoryer.Migrations.Postgres")),
            _ => throw new NotSupportedException(driver)
        };

        return new MainDbContext(builder.Options, Microsoft.Extensions.Options.Options.Create(new AuthOptions()));
    }
}
