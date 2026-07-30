using Microsoft.Extensions.Logging;

using Encore.Server;

using Mozart.Messages.Requests;
using Mozart.Messages.Responses;
using Mozart.Sessions;

namespace Mozart.Controllers;

[Authorize]
public class MusicShopController(Session session, ILogger<MusicShopController> logger) : CommandController(session)
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
