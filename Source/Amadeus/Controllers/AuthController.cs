using System.Security.Cryptography;
using Amadeus.Messages.Requests;
using Amadeus.Messages.Responses;
using Encore.Server;
using Microsoft.Extensions.Logging;
using Mozart.Data.Repositories;
using Mozart.Services;
using Mozart.Sessions;
using Session = Mozart.Sessions.Session;

namespace Amadeus.Controllers;

public class AuthController(
    Session session,
    ISessionManager manager,
    IAuthService authService,
    IChannelService channelService,
    IUserRepository userRepository,
    IOptions<GameOptions> gameOptions,
    ILogger<AuthController> logger
) : CommandController<Session>(session)
{
    [CommandHandler]
    public async Task<AuthResponse> Authorize(AuthRequest request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation((int)RequestCommand.Authorize,
                "Authorize session (ClientID: {ClientId})", request.ClientId);

            const StringComparison comparison = StringComparison.InvariantCultureIgnoreCase;
            var existingSession = channelService.Sessions.FirstOrDefault(s => s.Actor.Token.Equals(request.Token, comparison));
            if (existingSession != null)
            {
                if (!manager.Validate(existingSession))
                {
                    if (existingSession.Channel != null)
                        existingSession.Exit(existingSession.Channel!);

                    if (existingSession.Room != null)
                        existingSession.Exit(existingSession.Room!);
                }
                else
                {
                    return new AuthResponse
                    {
                        Result = AuthResult.ClientDuplicateSessions,
                        Subscription = new AuthResponse.SubscriptionInfo()
                    };
                }
            }

            // TODO: Also check the request.UserId when authorizing
            var authSession = await authService.Authorize(request.Token, cancellationToken);
            var characterInfo = await userRepository.Find(authSession.UserId, cancellationToken);

            if (characterInfo == null)
            {
                return new AuthResponse
                {
                    Result = AuthResult.DatabaseError,
                    Subscription = new AuthResponse.SubscriptionInfo()
                };
            }

            Session.Authorize(new Actor(characterInfo)
            {
                Token = authSession.Token,
                ClientId = request.ClientId
            });
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning((int)RequestCommand.Authorize, ex, "Failed to authorize [{token}]", request.Token);
            return new AuthResponse
            {
                Result = AuthResult.MemberTableQueryError,
                Subscription = new AuthResponse.SubscriptionInfo()
            };
        }

        var actor = Session.Actor;
        bool freeMusic = gameOptions.Value.FreeMusic;

        manager.CancelExpiry(Session);
        return new AuthResponse
        {
            Result = AuthResult.Success,
            Subscription = new AuthResponse.SubscriptionInfo
            {
                Type   = freeMusic ? FreePassType.AllMusic : actor.FreePass.Type,
                Expiry = !freeMusic && actor.FreePass.Type != FreePassType.None
                    ? actor.FreePass.ExpiryDate.ToUniversalTime() - DateTime.UtcNow
                    : TimeSpan.Zero
            },
            StarterPass = new AuthResponse.StarterPassInfo
            {
                Active = actor.StarterPass,
                Expiry = actor.StarterPassExpiryDate.HasValue
                    ? actor.StarterPassExpiryDate.Value.ToUniversalTime() - DateTime.UtcNow
                    : TimeSpan.Zero
            },
            AcquiredMusicIds = actor.AcquiredMusicIds.Select(i => (int)i).ToList()
        };
    }

    [CommandHandler(RequestCommand.SessionKeys)]
    public SessionKeysResponse GenerateSessionKeys()
    {
        Session.Properties["SessionKeys.Primary"]   = RandomNumberGenerator.GetBytes(32);
        Session.Properties["SessionKeys.Secondary"] = RandomNumberGenerator.GetBytes(16);

        return new SessionKeysResponse
        {
            Seed         = 0,
            Prefix       = 0,
            PrimaryKey   = (byte[])Session.Properties["SessionKeys.Primary"],
            SecondaryKey = (byte[])Session.Properties["SessionKeys.Secondary"]
        };
    }

    [CommandHandler(RequestCommand.Terminate)]
    public async Task Terminate(CancellationToken cancellationToken)
    {
        if (Session.Authorized)
        {
            logger.LogInformation((int)RequestCommand.Terminate, "Session stop requested");

            if (Session.Room != null)
                Session.Exit(Session.Room);

            if (Session.Channel != null)
                Session.Exit(Session.Channel);
        }

        await manager.StopSession(Session);
    }

    [CommandHandler(GenericCommand.LegacyPing, GenericCommand.LegacyPing)]
    public void LegacyPing()
    {
    }

    [Authorize]
    [CommandHandler(GenericCommand.Ping)]
    public PingResponse Ping()
    {
        return new PingResponse();
    }
}
