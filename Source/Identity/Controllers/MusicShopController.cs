using System.Text;
using Identity.Controllers.Filters;
using Identity.Messages.Responses;
using Encore.Server;
using Microsoft.Extensions.Logging;
using Mozart.Data.Repositories;
using Mozart.Sessions;

namespace Identity.Controllers;

[ChannelAuthorize]
public class MusicShopController(
    Session session,
    IUserRepository repository,
    ILogger<MusicShopController> logger
) : CommandController<Session>(session)
{
    [CommandHandler(RequestCommand.SyncMusicPurchase)]
    public async Task<SyncMusicPurchaseResponse> SyncPurchase(CancellationToken cancellationToken)
    {
        var actor = Session.Actor;
        logger.LogInformation((int)RequestCommand.SyncItemPurchase,
            "Sync music purchase");

        // The actual transaction happen within the web page, we only need to sync the latest user info
        var user = (await repository.Find(actor.UserId, cancellationToken))!;
        actor.Sync(user);

        return new SyncMusicPurchaseResponse
        {
            Gem       = user.Gem,
            Point     = user.Point,
            O2Cash    = user.O2Cash,
            MusicIds  = [],
            MusicCash = user.MusicCash,
            ItemCash  = user.ItemCash
        };
    }
}
