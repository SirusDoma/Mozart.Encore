using Encore.Server;
using Encore.Sessions;
using Microsoft.Extensions.Logging;
using Mozart.Sessions;

namespace Memoryer.Controllers.Filters;

public class InternalLoggerFilter(ILogger<InternalLoggerFilter> logger) : CommandFilter
{
    private IDisposable? _scope = null;

    public override void OnActionExecuting(CommandExecutingContext context)
    {
        if (context.Session is not ITcpSession { Authorized: true } tcp || tcp.GetAuthorizedToken() is not Actor)
            _scope = logger.BeginScope("System / Internal");
    }

    public override void OnActionExecuted(CommandExecutedContext context)
    {
        _scope?.Dispose();
    }
}
