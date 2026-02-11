using Encore.Server;
using Microsoft.Extensions.Logging;
using Amadeus.Messages.Requests;
using Mozart.Sessions;

namespace Amadeus.Controllers.Filters;

public class InternalLoggerFilter(ILogger<InternalLoggerFilter> logger) : CommandFilter
{
    private IDisposable? _scope = null;

    public override void OnActionExecuting(CommandExecutingContext context)
    {
        if (!context.Session.Authorized || context.Session.GetAuthorizedToken() is not Actor)
            _scope = logger.BeginScope("System / Internal");
    }

    public override void OnActionExecuted(CommandExecutedContext context)
    {
        _scope?.Dispose();
    }
}
