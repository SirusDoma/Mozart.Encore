namespace Encore.Server;

public interface ICommandFilter
{
    Task OnActionExecutingAsync(CommandExecutingContext context, CancellationToken cancellationToken = default);

    Task OnActionExecutedAsync(CommandExecutedContext context, CancellationToken cancellationToken = default);
}

public abstract class CommandFilter : ICommandFilter
{
    public int Order { get; set; } = 0;

    public virtual void OnActionExecuting(CommandExecutingContext context)
    {
    }

    public virtual void OnActionExecuted(CommandExecutedContext context)
    {
    }

    public virtual Task OnActionExecutingAsync(CommandExecutingContext context, CancellationToken cancellationToken = default)
    {
        OnActionExecuting(context);
        return Task.CompletedTask;
    }

    public virtual Task OnActionExecutedAsync(CommandExecutedContext context, CancellationToken cancellationToken = default)
    {
        OnActionExecuted(context);
        return Task.CompletedTask;
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public abstract class CommandFilterAttribute : Attribute, ICommandFilter
{
    public int Order { get; set; } = 0;

    public virtual void OnActionExecuting(CommandExecutingContext context)
    {
    }

    public virtual void OnActionExecuted(CommandExecutedContext context)
    {
    }

    public virtual Task OnActionExecutingAsync(CommandExecutingContext context, CancellationToken cancellationToken = default)
    {
        OnActionExecuting(context);
        return Task.CompletedTask;
    }

    public virtual Task OnActionExecutedAsync(CommandExecutedContext context, CancellationToken cancellationToken = default)
    {
        OnActionExecuted(context);
        return Task.CompletedTask;
    }
}
