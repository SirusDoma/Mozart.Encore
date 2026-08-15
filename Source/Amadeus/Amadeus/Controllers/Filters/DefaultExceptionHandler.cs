using Amadeus.Messages.Responses;
using Encore.Server;
using Encore.Sessions;

namespace Amadeus.Controllers.Filters;

public class DefaultExceptionHandler : CommandExceptionHandler
{
    public override void Handle(CommandExceptionHandlerContext context)
    {
        if (context.Session is ITcpSession { Authorized: false })
        {
            if (context.Exception is InvalidOperationException)
            {
                context.Result = new AuthResponse
                {
                    Result = AuthResult.NetworkError
                };
            }
        }

        // Suppress exception, prevent leaking outside command dispatcher
        context.Handled = true;
    }
}
