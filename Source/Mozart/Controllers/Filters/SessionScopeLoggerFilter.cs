using Encore.Server;
using Microsoft.Extensions.Logging;
using Mozart.Messages.Requests;

namespace Mozart.Controllers.Filters;

public class SessionScopeLoggerFilter(ILogger<SessionScopeLoggerFilter> logger) : CommandFilter
{
    private IDisposable? _scope = null;

    public override void OnActionExecuting(CommandExecutingContext context)
    {
        _scope = null;
        if (context.Session.Authorized)
            _scope = logger.BeginScope(context.Session.GetAuthorizedToken().ToString()!);
        else if (context.Request is AuthRequest request)
            _scope = logger.BeginScope(request.Token);
    }

    public override void OnActionExecuted(CommandExecutedContext context)
    {
        _scope?.Dispose();
    }
}