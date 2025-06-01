using Encore.Messaging;
using Encore.Sessions;

namespace Encore.Server;

public class CommandExecutionContext
{
    public CommandExecutionContext(Session session, IMessage? request, CommandHandlerDescriptor descriptor)
    {
        Session    = session ?? throw new ArgumentNullException(nameof(session));
        Request    = request;
        Descriptor = descriptor;
    }

    public Session Session { get; }

    public IMessage? Request { get; }

    public CommandHandlerDescriptor Descriptor { get; }

    public IMessage? Result { get; set; }
}

public class CommandExecutingContext : CommandExecutionContext
{
    public CommandExecutingContext(CommandExecutionContext context) :
        base(context.Session, context.Request, context.Descriptor)
    {
    }

    public bool Cancel { get; set; } = false;
}

public class CommandExecutedContext : CommandExecutionContext
{
    public CommandExecutedContext(CommandExecutionContext context, Exception? ex) :
        base(context.Session, context.Request, context.Descriptor)
    {
        Exception = ex;
    }

    public Exception? Exception { get; set; }

    public bool ExceptionHandled { get; set; } = false;
} 