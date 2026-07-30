using System.Text;
using CrossTime.Controllers.Filters;
using CrossTime.Messages.Requests;
using CrossTime.Messages.Responses;
using Encore.Server;
using Microsoft.Extensions.Logging;
using Mozart.Data.Repositories;
using Mozart.Entities;
using Mozart.Sessions;

namespace CrossTime.Controllers;

[ChannelAuthorize]
public class MusicShopController(
    Session session,
    IUserRepository repository,
    ILogger<MusicShopController> logger
) : CommandController<Session>(session)
{
    [CommandHandler]
    public void SyncMusicDownload(SyncMusicDownloadRequest request)
    {
        logger.LogInformation((int)RequestCommand.SyncMusicDownload, "Sync music install state");

        Session.Actor.InstalledMusicIds.Add(request.MusicId);
        if (Session.Room != null)
        {
            var slots = Session.Room.Slots.ToList();
            int memberId = slots.FindIndex(s => s is Room.MemberSlot m && m.Session == Session);

            Session.Room.UpdateMusicState(Session, memberId);
        }
    }

    [CommandHandler(RequestCommand.SyncMusicPurchase)]
    public async Task<SyncMusicPurchaseResponse> SyncPurchase(CancellationToken cancellationToken)
    {
        var actor = Session.Actor;
        logger.LogInformation((int)RequestCommand.SyncMusicPurchase,
            "Sync music purchase");

        // The actual transaction happen within the web page, we only need to sync the latest user info
        var user = (await repository.Find(actor.UserId, cancellationToken))!;
        actor.Sync(user);

        return new SyncMusicPurchaseResponse
        {
            Gem       = user.Gem,
            Point     = user.Point,
            O2Cash    = 0,
            MusicIds  = user.AcquiredMusicList.Select(m => (ushort)m.MusicId).ToList(),
            ItemCash  = 0,
            MusicCash = 0
        };
    }
}
