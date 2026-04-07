using Encore;
using Mozart.Options;

namespace Amadeus;

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
