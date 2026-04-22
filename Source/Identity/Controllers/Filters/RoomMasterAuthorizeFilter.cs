using Encore.Server;
using Mozart.Entities;
using Mozart.Metadata;
using Mozart.Sessions;

namespace Memoryer.Controllers.Filters;

public class RoomMasterAuthorizeAttribute: RoomAuthorizeAttribute
{
    public override void OnActionExecuting(CommandExecutingContext context)
    {
        base.OnActionExecuting(context);

        var session = (Session)context.Session;
        var role    = session.Room!.Slots.OfType<Room.MemberSlot>().Single(m => m.Session == session).LiveRole;
        if (session != session.Room!.Master && role != RoomLiveRole.Champion && role != RoomLiveRole.Challenger)
            throw new InvalidOperationException("Unauthorized");
    }
}
