using Encore.Server;

namespace Mozart;

public class DefaultExceptionHandler : CommandExceptionHandler
{
    public override void Handle(CommandExceptionHandlerContext context)
    {
        if (!context.Session.Authorized && context.Exception is InvalidOperationException)
        {
            context.Result = new ForceReauthorizeResponse();
        }

        // Suppress exception, prevent leaking outside command dispatcher
        context.Handled = true;
    }
}