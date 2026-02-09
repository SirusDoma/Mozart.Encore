using Encore.Server;

using Amadeus.Messages.Requests;
using Amadeus.Messages.Responses;
using Microsoft.Extensions.Logging;
using Mozart.Data.Repositories;
using Mozart.Sessions;

namespace Amadeus.Controllers;

[Authorize]
public class MusicShopController(
    Session session,
    IUserRepository repository,
    ILogger<ItemShopController> logger
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
            O2Cash    = 0,
            MusicIds  = [],
            ItemCash  = 0,
            MusicCash = 0
        };
    }
}