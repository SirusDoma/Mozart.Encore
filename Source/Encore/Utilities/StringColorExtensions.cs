namespace Encore;

public static class StringColorExtensions
{
    public static string WithConsoleColor(this string text, ConsoleColor color)
    {
        return $"\u001b[{GetConsoleColorCode(color)}m{text}\u001b[0m";
    }

    private static string GetConsoleColorCode(ConsoleColor color)
    {
        return color switch
        {
            ConsoleColor.Black       => "30",
            ConsoleColor.DarkRed     => "31",
            ConsoleColor.DarkGreen   => "32",
            ConsoleColor.DarkYellow  => "33",
            ConsoleColor.DarkBlue    => "34",
            ConsoleColor.DarkMagenta => "35",
            ConsoleColor.DarkCyan    => "36",
            ConsoleColor.Gray        => "37",
            ConsoleColor.DarkGray    => "90",
            ConsoleColor.Red         => "91",
            ConsoleColor.Green       => "92",
            ConsoleColor.Yellow      => "93",
            ConsoleColor.Blue        => "94",
            ConsoleColor.Magenta     => "95",
            ConsoleColor.Cyan        => "96",
            ConsoleColor.White       => "97",
            _                           => "37" // Default to gray
        };
    }
}