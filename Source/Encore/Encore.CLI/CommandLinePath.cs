namespace Encore.CLI;

public static class CommandLinePath
{
    public static string GetFullPath(string path)
    {
        if (OperatingSystem.IsWindows())
            path = path.Trim('"');

        return Path.GetFullPath(path);
    }
}
