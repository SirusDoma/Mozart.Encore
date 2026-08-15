using CrossTime.Messages.Responses;
using Encore.Server;
using Encore.Sessions;

namespace CrossTime.Controllers.Filters;

public class DefaultExceptionHandler : CommandExceptionHandler
{
    public override void Handle(CommandExceptionHandlerContext context)
    {
        if (context.Session is ITcpSession { Authorized: false })
        {
            if (context.Exception is InvalidOperationException)
            {
                context.Result = new AuthResponse { Result = AuthSessionResult.InvalidParameter };
            }
        }

        // Suppress exception, prevent leaking outside command dispatcher
        context.Handled = true;
    }
}
