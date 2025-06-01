using Encore.Server;
using Encore.Sessions;

namespace Mozart;

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