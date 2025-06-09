using Encore;

namespace Mozart.Options;

public enum DatabaseDriver
{
    Memory,
    Sqlite,
    SqlServer,
    MySql,
    Postgres,
    MongoDb,
    Couchbase
}

public class DatabaseOptions
{
    public const string Section = "Db";

    public required DatabaseDriver Driver { get; init; } = DatabaseDriver.Sqlite;
    public required string Url { get; init; } = "Data Source=O2JAM.db";
    public string Name { get; init; } = "O2JAM";

    public int? MinBatchSize { get; init; } = null;
    public int? MaxBatchSize { get; init; } = null;
    public int? CommandTimeout { get; init; } = null;
}

public static class DatabaseDriverExtensions
{
    public static string GetPrintableName(this DatabaseDriver driver)
    {
        return driver switch
        {
            DatabaseDriver.Memory     => "In-Memory".WithConsoleColor(ConsoleColor.DarkGray),
            DatabaseDriver.Sqlite     => "SQL Lite".WithConsoleColor(ConsoleColor.Cyan),
            DatabaseDriver.SqlServer  => "SQL Server".WithConsoleColor(ConsoleColor.Red),
            DatabaseDriver.MySql      => "MySQL".WithConsoleColor(ConsoleColor.Blue),
            DatabaseDriver.Postgres   => "PostgreSQL".WithConsoleColor(ConsoleColor.DarkBlue),
            DatabaseDriver.MongoDb    => "MongoDB".WithConsoleColor(ConsoleColor.Green),
            DatabaseDriver.Couchbase  => "Couchbase".WithConsoleColor(ConsoleColor.DarkRed),
            _ => throw new ArgumentOutOfRangeException(nameof(driver), driver.ToString())
        };
    }
}
