using System.Collections;

namespace Encore.Server;

public interface ICommandExceptionLogger
{
    Task LogAsync(CommandExceptionLoggerContext context, CancellationToken cancellationToken);
}

public abstract class CommandExceptionLogger : ICommandExceptionLogger
{
    internal const string LoggedByKey = "__private__/ENCORE_EXCEPTION_LOGGED_BY";

    Task ICommandExceptionLogger.LogAsync(CommandExceptionLoggerContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        ArgumentNullException.ThrowIfNull(context.ExceptionContext, nameof(context));

        if (!ShouldLog(context.ExceptionContext))
        {
            return Task.CompletedTask;
        }

        return LogAsync(context, cancellationToken);
    }

    public virtual Task LogAsync(CommandExceptionLoggerContext context, CancellationToken cancellationToken)
    {
        Log(context);
        return Task.CompletedTask;
    }

    public virtual void Log(CommandExceptionLoggerContext context)
    {
    }

    public virtual bool ShouldLog(CommandExceptionContext context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        var data = context.Exception.Data;
        if ((IDictionary?)data == null || data.IsReadOnly)
        {
            // If the exception doesn't have a mutable Data collection, we can't prevent duplicate logging. In this
            // case, just log every time.
            return true;
        }

        ICollection<object>? loggedBy;
        if (data.Contains(LoggedByKey))
        {
            object? untypedLoggedBy = data[LoggedByKey];
            loggedBy = untypedLoggedBy as ICollection<object>;

            if (loggedBy == null)
            {
                // If exception.Data exists but is not of the right type, we can't prevent duplicate
                // logging. In this case, just log every time.
                return true;
            }

            if (loggedBy.Contains(this))
            {
                // If this logger has already logged this exception, don't log again.
                return false;
            }
        }
        else
        {
            loggedBy = new List<object>();
            data.Add(LoggedByKey, loggedBy);
        }

        // Either loggedBy did not exist before (we just added it) or it already existed of the right type and did
        // not already contain this logger. Log now, but mark not to log this exception again for this logger.
        loggedBy.Add(this);
        return true;
    }
}