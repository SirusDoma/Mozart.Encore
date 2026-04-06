using System.Security.Cryptography;
using Identity.Messages.Requests;
using Identity.Messages.Responses;
using Mozart.Data.Entities;
using Encore.Server;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mozart.Data.Repositories;
using Mozart.Options;
using Mozart.Services;
using Mozart.Sessions;
using Session = Mozart.Sessions.Session;

namespace Identity.Controllers;

public class AuthController(
    Session session, ISessionManager manager,
    IAuthService authService,
    IChannelService channelService,
    IUserRepository userRepository,
    IMetadataResolver resolver,
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
                        Result = AuthResult.AlreadyConnected
                    };
                }
            }

            var authSession = await authService.Authorize(request.Token, cancellationToken);
            var characterInfo = await userRepository.Find(authSession.UserId, cancellationToken);

            if (characterInfo == null)
            {
                return new AuthResponse
                {
                    Result = AuthResult.DatabaseError
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
                Result = AuthResult.InvalidLogin
            };
        }

        var actor = Session.Actor;
        bool freeMusic = gameOptions.Value.FreeMusic;

        // TODO: Limit to 100?
        actor.Top100 = resolver.GetMusicList().Values
                .OrderByDescending(m => m.ReleaseDate)
                .ThenByDescending(m => m.Id)
                .Where(m => m.PriceO2Cash != 0)
                .Select(m => m.Id)
                .ToList();

        manager.CancelExpiry(Session);
        return new AuthResponse
        {
            Result = AuthResult.Success,
            FreePass = new AuthResponse.FreePassInfo
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
            AcquiredMusicIds = actor.Top100
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
    [CommandHandler(GenericCommand.Ping, GenericCommand.Ping)]
    public void Ping()
    {
    }
}
