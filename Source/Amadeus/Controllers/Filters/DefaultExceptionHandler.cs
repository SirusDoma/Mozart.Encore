using Amadeus.Messages.Responses;
using Encore.Server;

namespace Amadeus.Controllers.Filters;

public class DefaultExceptionHandler : CommandExceptionHandler
{
    public override void Handle(CommandExceptionHandlerContext context)
    {
        if (!context.Session.Authorized)
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
