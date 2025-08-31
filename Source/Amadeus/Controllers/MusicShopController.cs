using Encore.Server;

using Amadeus.Messages.Requests;
using Amadeus.Messages.Responses;
using Mozart.Sessions;

namespace Amadeus.Controllers;

[Authorize]
public class MusicShopController(Session session) : CommandController(session)
{
    [CommandHandler]
    public PurchaseMusicResponse PurchaseMusic(PurchaseMusicRequest request)
    {
        return new PurchaseMusicResponse
        {
            Result = PurchaseMusicResponse.PurchaseResult.Success,
        };
    }
}