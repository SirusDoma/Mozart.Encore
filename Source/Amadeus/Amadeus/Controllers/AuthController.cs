using System.Security.Cryptography;
using Amadeus.Messages.Requests;
using Amadeus.Messages.Responses;
using Encore.Data.Repositories;
using Encore.Server;
using Encore.Server.Sessions;
using Encore.Services;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Mozart.Sessions;

namespace Amadeus.Controllers;

public class AuthController(
    Session session,
    ISessionManager manager,
    IAuthService authService,
    IChannelService channelService,
    IUserRepository userRepository,
    ILogger<AuthController> logger
) : CommandController<Session>(session)
{
    [CommandHandler]
    public async Task<AuthResponse> Authorize(AuthRequest request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation((int)RequestCommand.Authorize,
                "Authorize session (Client: v{Version}, ClientID: {ClientId})", request.ClientVersion, request.Credential is AuthRequest.GamaniaCredential g ? g.Unknown : request.ClientId);

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
                        Result = AuthResult.ClientDuplicateSessions
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
                Result = AuthResult.MemberTableQueryError
            };
        }

        manager.CancelExpiry(Session);
        return new AuthResponse
        {
            Result = AuthResult.Success,
            Subscription = request.ClientId.IsNullOrEmpty() ? null : new AuthResponse.SubscriptionInfo
            {
                Billing                   = BillingCode.HB,
                CurrentTimestamp          = DateTime.Now,
                SubscriptionRemainingTime = TimeSpan.FromMinutes(0)
            }
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
