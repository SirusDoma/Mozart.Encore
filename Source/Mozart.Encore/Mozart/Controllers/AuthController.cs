using Microsoft.Extensions.Logging;
using Encore.Server;
using Encore.Sessions;

namespace Mozart;

public class AuthController : CommandController
{
    private readonly ILogger<AuthController> _logger;

    public AuthController(Session session, ILogger<AuthController> logger)
        : base(session)
    {
        _logger = logger;
    }

    [CommandHandler]
    public Task<AuthResponse> Authorize(AuthRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Authorizing token: {request.Token}");
        Session.Authorize(new CharacterInfo
        {
            Token            = request.Token,
            Nickname         = "Mozart",
            Gender           = Gender.Male,
            Gem              = 1500,
            Point            = 2500,
            Level            = 100,
            Win              = 1660,
            Lose             = 722,
            Draw             = 673,
            Experience       = 1500,
            IsAdministrator  = true,
            Equipments       = new Dictionary<ItemType, int>
            {
                [ItemType.Instrument]         = 239,
                [ItemType.Hair]               = 009,
                [ItemType.Earring]            = 044,
                [ItemType.Gloves]             = 070,
                [ItemType.Accessories]        = 346, // a.k.a Ring
                [ItemType.Top]                = 130,
                [ItemType.Pants]              = 192,
                [ItemType.Glasses]            = 084,
                [ItemType.Necklace]           = 056,
                [ItemType.ClothesAccessories] = 000, // a.k.a Bracelet
                [ItemType.Shoes]              = 292,
                [ItemType.Face]               = 245
            },
            Inventory = new List<int>([41]).Concat(Enumerable.Repeat(0, 30 - 1)).ToList()
        });
        
        return Task.FromResult(new AuthResponse
        {
            Result = AuthResult.Success,
            TimeBlockSubscription = new AuthResponse.TimeBlockSubscriptionInfo
            {
                Billing                   = BillingCode.DB,
                CurrentTimestamp          = DateTime.Now,
                SubscriptionRemainingTime = TimeSpan.FromMinutes(240)
            }
        });
    }

    [CommandHandler(RequestCommand.Terminate)]
    public Task Terminate(CancellationToken cancellationToken)
    {
        if (Session.Authorized)
            _logger.LogInformation($"Terminating session: {Session.GetAuthorizedToken<CharacterInfo>().Token}");

        return Task.CompletedTask;
    }

    [CommandHandler(GenericCommand.LegacyPing)]
    public LegacyPingResponse LegacyPing()
    {
        return new LegacyPingResponse();
    }

    [Authorize]
    [CommandHandler(GenericCommand.Ping)]
    public PingResponse Ping()
    {
        return new PingResponse();
    }
}