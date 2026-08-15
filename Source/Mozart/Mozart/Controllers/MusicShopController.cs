using Encore.Server;
using Encore.Server.Sessions;
using Microsoft.Extensions.Logging;
using Mozart.Messages.Requests;
using Mozart.Messages.Responses;
using Mozart.Sessions;

namespace Mozart.Controllers;

[Authorize]
public class MusicShopController(Session session, ILogger<MusicShopController> logger) : CommandController<Session>(session)
{
    [CommandHandler]
    public void SyncMusicDownload(SyncMusicDownloadRequest request)
    {
        logger.LogInformation((int)RequestCommand.SyncMusicDownload,
            "Sync music install state");

        var actor = Session.GetAuthorizedToken<Actor>();
        actor.MusicIds = [..actor.MusicIds, (int)request.MusicId];
    }

    [CommandHandler]
    public PurchaseMusicResponse PurchaseMusic(PurchaseMusicRequest request)
    {
        return new PurchaseMusicResponse
        {
            Result = PurchaseMusicResponse.PurchaseResult.Success,
        };
    }
}
