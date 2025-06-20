using Encore.Server;
using Microsoft.Extensions.Logging;
using Mozart.Messages.Requests;
using Mozart.Sessions;

namespace Mozart.Controllers.Filters;

public class SessionScopeLoggerFilter(ILogger<SessionScopeLoggerFilter> logger) : CommandFilter
{
    private IDisposable? _scope = null;

    public override void OnActionExecuting(CommandExecutingContext context)
    {
        if (Enum.GetValues<GatewayCommand>().Contains((GatewayCommand)context.Command))
            return;

        _scope = null;
        if (context.Session.Authorized && context.Session.GetAuthorizedToken() is Actor actor)
            _scope = logger.BeginScope(actor.Nickname);
        else if (context.Request is AuthRequest request)
            _scope = logger.BeginScope("System / Auth");
    }

    public override void OnActionExecuted(CommandExecutedContext context)
    {
        _scope?.Dispose();
    }
}