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
            Result = SyncFreePassResponse.SyncResult.Success,
            Info   = new SyncFreePassResponse.UserFreePassInfo
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
            }
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
