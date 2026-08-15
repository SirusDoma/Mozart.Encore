using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace Encore.Hosting.Logging;

public class EncoreConsoleFormatterOptions : ConsoleFormatterOptions
{
    public int EventWidth { get; init; } = 4;
    public int CategoryWidth { get; init; } = 55;
    public int StateWidth { get; init; } = 45;
    public ConsoleColor CategoryColor { get; init; } = ConsoleColor.Cyan;
    public ConsoleColor StateColor { get; init; } = ConsoleColor.Yellow;
}

public class EncoreConsoleFormatter : ConsoleFormatter
{
    private readonly EncoreConsoleFormatterOptions _options;

    public EncoreConsoleFormatter(IOptionsMonitor<EncoreConsoleFormatterOptions> options)
        : base("EncoreLoggerFormatter")
    {
        _options = options.CurrentValue;
    }

    public override void Write<TState>(in LogEntry<TState> logEntry,
        IExternalScopeProvider? scopeProvider, TextWriter textWriter)
    {
        string? message = logEntry.Formatter?.Invoke(logEntry.State, logEntry.Exception);

        if (string.IsNullOrEmpty(message))
            return;

        string timestamp = DateTimeOffset.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");

        string levelText = GetColoredLogLevel(logEntry.LogLevel);

        string eventId = PadAndColorText(logEntry.EventId.Id.ToString($"X{_options.EventWidth}"), _options.EventWidth,
            ConsoleColor.Gray, '0');

        string category = PadAndColorText(logEntry.Category, _options.CategoryWidth, _options.CategoryColor);

        string state = GetFormattedState(scopeProvider, _options.StateWidth, _options.StateColor);

        textWriter.WriteLine($"{timestamp} {levelText} {eventId} --- [ {category} ] [ {state} ]: {message}");

        if (logEntry.Exception != null)
        {
            textWriter.WriteLine(logEntry.Exception.ToString());
        }
    }

    private static string GetColoredLogLevel(LogLevel logLevel)
    {
        var levelText = GetLogLevelText(logLevel);
        var colorCode = GetLogLevelColorCode(logLevel);

        return $"\u001b[{colorCode}m{levelText}\u001b[0m";
    }

    private static string GetLogLevelText(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.None        => string.Empty,
            LogLevel.Trace       => "TRACE",
            LogLevel.Debug       => "DEBUG",
            LogLevel.Information => "INFO ",
            LogLevel.Warning     => "WARN ",
            LogLevel.Error       => "ERROR",
            LogLevel.Critical    => "FATAL",
            _                    => logLevel.ToString().ToUpper()
        };
    }

    private static string GetLogLevelColorCode(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace       => "37", // Gray
            LogLevel.Debug       => "37", // Gray
            LogLevel.Information => "32", // Green
            LogLevel.Warning     => "33", // Yellow
            LogLevel.Error       => "31", // Red
            LogLevel.Critical    => "35", // Magenta
            _                    => "37"  // White
        };
    }

    private static string PadAndColorText(string text, int padding, ConsoleColor color, char paddingChar = ' ')
    {
        string paddedText = text.Length > padding ? text : text.PadRight(padding, paddingChar);
        return paddedText.WithConsoleColor(color);
    }

    private static string GetFormattedState(IExternalScopeProvider? scopeProvider, int padding, ConsoleColor color)
    {
        var states = new List<string>();
        scopeProvider?.ForEachScope<object>((scope, _) =>
        {
            string? name = scope?.ToString();
            if (!string.IsNullOrEmpty(name))
                states.Add(name);
        }, true);

        string state = states.Count > 0 ? $"{string.Join(" / ", states)}" : string.Empty;
        return PadAndColorText(state, padding, color);
    }
}
