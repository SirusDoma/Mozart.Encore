using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

namespace Encore.Hosting.Logging;

public class EncoreConsoleFormatter: ConsoleFormatter
{
    public EncoreConsoleFormatter()
        : base("EncoreLoggerFormatter")
    {
    }

    public override void Write<TState>(
        in LogEntry<TState> logEntry,
        IExternalScopeProvider? scopeProvider,
        TextWriter textWriter)
    {
        var logLevel    = logEntry.LogLevel;
        string category = logEntry.Category;
        string? message = logEntry.Formatter?.Invoke(logEntry.State, logEntry.Exception);

        if (!string.IsNullOrEmpty(message))
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            textWriter.WriteLine($"[{timestamp}] [{logLevel}]: {category} - {message}");
        }
    }
}