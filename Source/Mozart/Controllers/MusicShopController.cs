using Encore.Server;

using Mozart.Messages.Requests;
using Mozart.Messages.Responses;
using Mozart.Sessions;

namespace Mozart.Controllers;

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