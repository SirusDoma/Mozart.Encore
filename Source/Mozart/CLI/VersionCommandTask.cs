namespace Mozart.CLI;

/// <summary>
/// Version command implementation
/// </summary>
public class VersionCommandTask : ICommandLineTask
{
    public static string Name => "version";

    public static string Description => "Display version information";

    public int Execute()
    {
        Console.WriteLine($"Mozart.Encore");
        Console.WriteLine($"[v] Version: {Program.Version}");
        Console.WriteLine($"[!] Network environment: {Program.NetworkVersion}");
        Console.WriteLine($"[?] Github: {Program.RepositoryUrl}");

        return 0;
    }
} 