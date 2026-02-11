using System.Text;
using Encore.Server;

using Amadeus.Controllers.Filters;
using Amadeus.Messages.Responses;

using Microsoft.Extensions.Logging;
using Mozart.Data.Repositories;
using Mozart.Sessions;

namespace Amadeus.Controllers;

[ChannelAuthorize]
public class MusicShopController(
    Session session,
    IUserRepository repository,
    ILogger<MusicShopController> logger
) : CommandController<Session>(session)
{

    [CommandHandler(RequestCommand.PurchasableMusic, ResponseCommand.PurchasableMusic)]
    public PurchasableMusicListResponse GetPurchasableMusicList()
    {
        logger.LogInformation((int)RequestCommand.StartPayment,
            "Get purchasable music");

        return new PurchasableMusicListResponse
        {
            Items = Session.Channel!.GetMusicList().Values.Where(m => m.IsPurchasable).Select(m =>
                new PurchasableMusicListResponse.MusicItem
                {
                    MusicId      = m.Id,
                    Title        = Encoding.UTF8.GetString(m.Title),
                    Artist       = Encoding.UTF8.GetString(m.Artist),
                    NoteDesigner = Encoding.UTF8.GetString(m.NoteDesigner),
                    OJM          = m.OJM
                }
            ).ToList()
        };
    }

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
