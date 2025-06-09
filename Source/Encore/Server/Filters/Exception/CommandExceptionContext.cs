using Encore.Messaging;
using Encore.Sessions;

namespace Encore.Server;

public class CommandExceptionContext
{
    public CommandExceptionContext(Exception exception, Session session, CommandHandlerDescriptor? descriptor,
        IMessage? request = null)
    {
        Exception  = exception;
        Session    = session;
        Descriptor = descriptor;
        Request    = request;
    }

    public Exception Exception { get; }

    public Session Session { get; }

    public CommandHandlerDescriptor? Descriptor { get; }

    public IMessage? Request { get; }
}

public class CommandExceptionHandlerContext
{
    public CommandExceptionHandlerContext(CommandExceptionContext context)
    {
        ExceptionContext = context;
    }

    public Exception Exception => ExceptionContext.Exception;

    public Session Session => ExceptionContext.Session;

    public CommandHandlerDescriptor? Descriptor => ExceptionContext.Descriptor;

    public IMessage? Request => ExceptionContext.Request;

    public CommandExceptionContext ExceptionContext { get; }

    public IMessage? Result { get; set; }

    public bool Handled { get; set; } = false;
}

public class CommandExceptionLoggerContext
{
    public CommandExceptionLoggerContext(CommandExceptionContext context)
    {
        ExceptionContext = context;
    }

    public Exception Exception => ExceptionContext.Exception;

    public Session Session => ExceptionContext.Session;

    public CommandHandlerDescriptor? Descriptor => ExceptionContext.Descriptor;

    public IMessage? Request => ExceptionContext.Request;

    public CommandExceptionContext ExceptionContext { get; }

    public bool PropagateException { get; set; } = true;
}