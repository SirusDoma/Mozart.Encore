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
