namespace Encore.Server;

public interface ICommandExceptionHandler
{
    Task HandleAsync(CommandExceptionHandlerContext context, CancellationToken cancellationToken);
}

public abstract class CommandExceptionHandler : ICommandExceptionHandler
{
    Task ICommandExceptionHandler.HandleAsync(CommandExceptionHandlerContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        ArgumentNullException.ThrowIfNull(context.ExceptionContext, nameof(context));

        if (!ShouldHandle(context.ExceptionContext))
        {
            return Task.CompletedTask;
        }

        return HandleAsync(context, cancellationToken);
    }

    public Task HandleAsync(CommandExceptionHandlerContext context, CancellationToken cancellationToken)
    {
        Handle(context);
        return Task.CompletedTask;
    }

    public virtual void Handle(CommandExceptionHandlerContext context)
    {
    }

    public virtual bool ShouldHandle(CommandExceptionContext context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        return true;
    }
}
