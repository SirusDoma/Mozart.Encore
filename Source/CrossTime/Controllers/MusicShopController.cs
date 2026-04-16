using System.Text;
using CrossTime.Controllers.Filters;
using CrossTime.Messages.Responses;
using Encore.Server;
using Identity.Messages.Events;
using Identity.Messages.Requests;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mozart.Data.Entities;
using Mozart.Data.Repositories;
using Mozart.Entities;
using Mozart.Metadata;
using Mozart.Options;
using Mozart.Sessions;

namespace CrossTime.Controllers;

[ChannelAuthorize]
public class MusicShopController(
    Session session,
    IOptions<GameOptions> gameOptions,
    IUserRepository repository,
    ILogger<MusicShopController> logger
) : CommandController<Session>(session)
{
    [CommandHandler]
    public async Task SyncMusicDownload(SyncMusicDownloadRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation((int)RequestCommand.SyncMusicDownload, "Sync music install state");

        Session.Actor.InstalledMusicIds.Add(request.MusicId);
        if (Session.Room != null)
        {
            Session.Room.UpdateMusicState(Session, MusicState.Ready);

            var slots = Session.Room.Slots.ToList();
            int memberId = slots.FindIndex(s => s is Room.MemberSlot m && m.Session == Session);

            await Session.Room.Broadcast(new MusicStateChangedEventData
            {
                MemberId = (byte)memberId,
                State    = ((Room.MemberSlot)slots[memberId]).MusicState
            }, cancellationToken);
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

        var expiry = DateTime.MinValue;
        bool free  = gameOptions.Value.FreeMusic || Session.Actor.FreePass.Type == FreePassType.AllMusic;

        var acquiredMusic = new HashSet<int>();
        if (!free)
        {
            // TODO: Support time-limited promotion/event?
            if (Session.Actor.FreePass.Type != FreePassType.None)
                acquiredMusic = [..actor.AcquiredMusicIds.Select(i => (int)i)];

            // FreePass in the original server implementation may a lot more complex than this.
            // But we have no way to know how it works now.
            expiry = Session.Actor.FreePass.ExpiryDate;
        }
        else
        {
            acquiredMusic = [..Session.Channel!.GetMusicList()
                .Where(m => m.Value.IsPurchasable).Select(m => m.Key)];
        }

        var extensionPeriod = TimeSpan.Zero;
        if (actor.FreePass.Type != FreePassType.None || user.FreePass.Type != FreePassType.None)
            extensionPeriod = (actor.FreePass.ExpiryDate - user.FreePass.ExpiryDate).Duration();

        return new SyncMusicPurchaseResponse
        {
            Gem          = user.Gem,
            Point        = user.Point,
            O2Cash       = user.O2Cash,
            MusicCash    = user.MusicCash,
            ItemCash     = user.ItemCash,
            CashPoint    = user.CashPoint,
            MusicList    = Session.Channel!.GetMusicList()
                .Where(m =>
                    m.Value.IsPurchasable
                    && (free || (acquiredMusic.Contains(m.Key) || expiry != DateTime.MinValue))
                )
                .Select(m =>
                    new MusicPremiumTimeEventData.MusicEntry
                    {
                        MusicId = (ushort)m.Key,
                        Day     = (byte)(free || acquiredMusic.Contains(m.Key) ? 0 : expiry.Day),
                        Month   = (byte)(free || acquiredMusic.Contains(m.Key) ? 0 : expiry.Month),
                        Year    = (byte)(free || acquiredMusic.Contains(m.Key) ? 0 : expiry.Year % 1000),
                        Hour    = (byte)(free || acquiredMusic.Contains(m.Key) ? 0 : expiry.Hour),
                        Minute  = (byte)(free || acquiredMusic.Contains(m.Key) ? 0 : expiry.Minute)
                    }
                ).ToList(),
            ItemGiftBox  = actor.GiftItems.Select(i =>
                new CharacterInfoResponse.GiftItemInfo
                {
                    GiftId = i.Id,
                    ItemId = i.ItemId,
                    Sender = i.SenderNickname
                }
            ).ToList(),
            MusicGiftBox = actor.GiftMusics.Select(m =>
                new CharacterInfoResponse.GiftMusicInfo
                {
                    GiftId  = m.Id,
                    MusicId = m.MusicId,
                    Sender  = m.SenderNickname
                }
            ).ToList(),
            FreePassExtensionPeriod = extensionPeriod
        };
    }
}
