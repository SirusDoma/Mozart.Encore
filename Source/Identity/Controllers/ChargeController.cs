using Identity.Controllers.Filters;
using Identity.Messages.Responses;
using Identity.Messages.Requests;
using Encore.Server;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mozart.Data.Entities;
using Mozart.Data.Repositories;
using Mozart.Options;
using Mozart.Sessions;

namespace Identity.Controllers;

[ChannelAuthorize]
public class ChargeController(
    Session session,
    IOptions<GameOptions> gameOptions,
    IUserRepository repository,
    ILogger<ChargeController> logger
) : CommandController<Session>(session)
{
    private const int FreePassPrice = 9900;

    [CommandHandler(RequestCommand.SyncCashPoint)]
    public async Task<SyncCashPointResponse> ChargeCashPoint(CancellationToken cancellationToken)
    {
        var actor = Session.GetAuthorizedToken<Actor>();
        logger.LogInformation((int)RequestCommand.SyncCashPoint, "Sync cash point");

        // The actual topup happen within the web page, we only need to sync the latest user info
        var user = (await repository.Find(actor.UserId, cancellationToken))!;
        actor.Sync(user);

        return new SyncCashPointResponse
        {
            Gem = actor.Gem,
            CashPoint = actor.CashPoint
        };
    }

    [CommandHandler(RequestCommand.SyncPoint)]
    public async Task<SyncPointResponse> ChargePoint(CancellationToken cancellationToken)
    {
        var actor = Session.Actor;
        logger.LogInformation((int)RequestCommand.SyncPoint, "Sync point");

        // The actual topup happen within the web page, we only need to sync the latest user info
        var user = (await repository.Find(actor.UserId, cancellationToken))!;
        actor.Sync(user);

        return new SyncPointResponse
        {
            Point = user.O2Cash
        };
    }

    [CommandHandler(RequestCommand.SyncFreePass)]
    public async Task<SyncFreePassResponse> SyncFreePass(CancellationToken cancellationToken)
    {
        var actor = Session.Actor;
        logger.LogInformation((int)RequestCommand.SyncFreePass, "Sync free pass");

        // The actual topup happen within the web page, we only need to sync the latest user info
        var user = (await repository.Find(actor.UserId, cancellationToken))!;
        actor.Sync(user);

        bool freeMusic = gameOptions.Value.FreeMusic;

        return new SyncFreePassResponse
        {
            Gem            = user.Gem,
            Point          = user.Point,
            O2Cash         = user.O2Cash,
            MusicCash      = user.MusicCash,
            ItemCash       = user.ItemCash,
            FreePassType   = freeMusic ? FreePassType.AllMusic : user.FreePass.Type,
            CashPoint      = user.CashPoint,
            FreePassExpiry = !freeMusic && user.FreePass.Type != FreePassType.None
                             ? user.FreePass.ExpiryDate.ToUniversalTime() - DateTime.UtcNow
                             : TimeSpan.Zero
        };
    }

    [CommandHandler]
    public async Task<PurchaseFreePassResponse> PurchaseFreePass(PurchaseFreePassRequest request,
        CancellationToken cancellationToken)
    {
        var actor = Session.Actor;
        logger.LogInformation((int)RequestCommand.PurchaseFreePass, "Purchase free pass");

        var user = (await repository.Find(actor.UserId, cancellationToken))!;

        int cash = user.O2Cash - FreePassPrice;
        if (cash < 0)
        {
            return new PurchaseFreePassResponse
            {
                ErrorCode = -1,
                ExtensionPeriod = TimeSpan.Zero
            };
        }

        bool extension = user.FreePass.Type != FreePassType.None;
        var type = extension ? user.FreePass.Type : FreePassType.FreePlay;
        var expiry = (extension
            ? user.FreePass.ExpiryDate
            : DateTime.UtcNow) + TimeSpan.FromDays(30);

        user.FreePass = new FreePass(type, expiry);
        user.O2Cash   = cash;

        await repository.Update(user, cancellationToken);
        await repository.Commit(cancellationToken);

        actor.Sync(user);

        return new PurchaseFreePassResponse
        {
            ErrorCode = 0,
            ExtensionPeriod = extension ? TimeSpan.FromDays(30) : TimeSpan.Zero
        };
    }

    [CommandHandler]
    public async Task<GiftFreePassResponse> GiftFreePass(GiftFreePassRequest request,
        CancellationToken cancellationToken)
    {
        var actor = Session.Actor;
        logger.LogInformation((int)RequestCommand.GiftFreePass, "Gift free pass");

        var user      = (await repository.Find(actor.UserId, cancellationToken))!;
        var recipient = (await repository.FindByNickname(request.Recipient, cancellationToken));

        if (recipient == null)
        {
            return new GiftFreePassResponse
            {
                ErrorCode = -2,
                ExtensionPeriod = TimeSpan.Zero
            };
        }

        int cash = user.O2Cash - FreePassPrice;
        if (cash < 0)
        {
            return new GiftFreePassResponse
            {
                ErrorCode = -1,
                ExtensionPeriod = TimeSpan.Zero
            };
        }

        bool extension = recipient.FreePass.Type != FreePassType.None;
        var type = extension ? recipient.FreePass.Type : FreePassType.FreePlay;
        var expiry = (extension
            ? recipient.FreePass.ExpiryDate
            : DateTime.UtcNow) + TimeSpan.FromDays(30);

        recipient.FreePass = new FreePass(type, expiry);
        user.O2Cash        = cash;

        await repository.Update(user, cancellationToken);
        await repository.Update(recipient, cancellationToken);
        await repository.Commit(cancellationToken);

        actor.Sync(user);

        return new GiftFreePassResponse
        {
            ErrorCode = 0,
            ExtensionPeriod = extension ? TimeSpan.FromDays(30) : TimeSpan.Zero
        };
    }

    [CommandHandler]
    public async Task<CheckGiftFreePassResponse> CheckGiftFreePass(CheckGiftFreePassRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation((int)RequestCommand.CheckGiftFreePass, "Check gift free pass: [{Name}]",
            request.Name);

        var recipient = await repository.FindByNickname(request.Name, cancellationToken);
        return new CheckGiftFreePassResponse
        {
            Invalid  = recipient == null,
            Username = recipient?.Nickname ?? string.Empty,
        };
    }

}
