using Encore.Metadata;
using Encore.Server;
using Encore.Server.Sessions;

namespace Memoryer.Controllers.Filters;

public class RoomMasterAuthorizeAttribute: RoomAuthorizeAttribute
{
    public override void OnActionExecuting(CommandExecutingContext context)
    {
        base.OnActionExecuting(context);

        var session = (Session)context.Session;
        var role    = session.Room!.Slots.OfType<Encore.Entities.Room.MemberSlot>().Single(m => m.Session == session).Role;
        if (session != session.Room!.Master && role != MemberRole.Champion && role != MemberRole.Challenger)
            throw new InvalidOperationException("Unauthorized");
    }
}
