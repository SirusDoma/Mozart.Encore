using Encore.Server;
using Mozart.Sessions;

namespace Amadeus.Controllers.Filters;

public class RoomAuthorizeAttribute: CommandFilterAttribute
{
    public override void OnActionExecuting(CommandExecutingContext context)
    {
        if (context.Session is not Session session)
            throw new InvalidOperationException("Invalid session type");

        if (session.Channel == null)
            throw new InvalidOperationException("Unauthorized channel access");

        if (session.Room == null)
            throw new InvalidOperationException("Unauthorized room access");
    }
}
