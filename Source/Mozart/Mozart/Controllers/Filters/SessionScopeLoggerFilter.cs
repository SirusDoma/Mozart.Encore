using Encore;
using Encore.Server;
using Encore.Sessions;
using Microsoft.Extensions.Logging;
using Mozart.Messages.Requests;
using Mozart.Sessions;

namespace Mozart.Controllers.Filters;

public class SessionScopeLoggerFilter(ILogger<SessionScopeLoggerFilter> logger) : CommandFilter
{
    private IDisposable? _scope = null;

    public override void OnActionExecuting(CommandExecutingContext context)
    {
        if (context.Command is ServerCommand or GameManagerCommand or GatewayCommand)
            return;

        _scope = null;
        if (context.Session is ITcpSession { Authorized: true } tcp && tcp.GetAuthorizedToken() is Actor actor)
            _scope = logger.BeginScope(actor.Nickname);
        else if (context.Request is AuthRequest request)
            _scope = logger.BeginScope("System / Auth");
    }

    public override void OnActionExecuted(CommandExecutedContext context)
    {
        _scope?.Dispose();
    }
}
