using Encore.Server;
using Mozart.Sessions;

namespace Mozart.Controllers.Filters;

public class ChannelAuthorizeAttribute : CommandFilterAttribute
{
    public override void OnActionExecuting(CommandExecutingContext context)
    {
        if (context.Session is not Session session)
            throw new InvalidOperationException("Invalid session type");

        if (session.Channel == null)
            throw new InvalidOperationException("Unauthorized channel access");

    }
}