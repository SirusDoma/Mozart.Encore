using Encore.Server;
using Mozart.Sessions;

namespace Amadeus.Controllers.Filters;

public class RoomMasterAuthorizeAttribute: RoomAuthorizeAttribute
{
    public override void OnActionExecuting(CommandExecutingContext context)
    {
        base.OnActionExecuting(context);

        var session = (Session)context.Session;
        if (session != session.Room!.Master)
            throw new InvalidOperationException("Unauthorized");
    }
}