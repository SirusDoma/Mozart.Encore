using Encore.Server;
using Memoryer.Relay.Sessions;
using Microsoft.Extensions.Logging;

namespace Memoryer.Relay.Controllers.Filters;

public class RelayLoggerFilter(ILogger<RelayLoggerFilter> logger) : CommandFilter
{
    private IDisposable? _scope = null;

    public override void OnActionExecuting(CommandExecutingContext context)
    {
        if (context.Session is TcpRelaySession)
            _scope = logger.BeginScope("System / Relay / TCP");
        else if (context.Session is UdpRelaySession or UdpRelayPeer)
            _scope = logger.BeginScope("System / Relay / UDP");
    }

    public override void OnActionExecuted(CommandExecutedContext context)
    {
        _scope?.Dispose();
    }
}
