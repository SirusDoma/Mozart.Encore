using Encore.Server;
using Encore.Sessions;
using Mozart.Messages.Responses;

namespace Mozart.Controllers.Filters;

public class DefaultExceptionHandler : CommandExceptionHandler
{
    public override void Handle(CommandExceptionHandlerContext context)
    {
        if (context.Session is ITcpSession { Authorized: false })
        {
            if (context.Exception is InvalidOperationException)
            {
                context.Result = new ForceReauthorizeResponse();
            }
        }

        // Suppress exception, prevent leaking outside command dispatcher
        context.Handled = true;
    }
}
