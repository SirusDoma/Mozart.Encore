using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;

using Encore.Server;
using Mozart.Data.Entities;
using Mozart.Data.Repositories;
using Mozart.Messages.Requests;
using Mozart.Messages.Responses;
using Mozart.Services;
using Mozart.Sessions;

using Session = Mozart.Sessions.Session;

namespace Mozart.Controllers;

public class AuthController(Session session, ISessionManager manager, IIdentityService identityService,
    IChannelService channelService, IUserRepository userRepository, ILogger<AuthController> logger) : CommandController<Session>(session)
{
    [CommandHandler]
    public async Task<AuthResponse> Authorize(AuthRequest request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation((int)RequestCommand.Authorize, "Authorize session");

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
                        Result = AuthResult.DuplicateSessions,
                        Subscription = new AuthResponse.SubscriptionInfo()
                    };
                }
            }

            var authSession = await identityService.Authorize(request.Token, cancellationToken);
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
                Token = authSession.Token
            });
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning((int)RequestCommand.Authorize, ex, "Failed to authorize [{token}]", request.Token);
            return new AuthResponse
            {
                Result = AuthResult.InvalidCredentials,
                Subscription = new AuthResponse.SubscriptionInfo()
            };
        }

        manager.CancelExpiry(Session);
        return new AuthResponse
        {
            Result = AuthResult.Success,
            Subscription = new AuthResponse.SubscriptionInfo
            {
                Billing                   = BillingCode.TB,
                CurrentTimestamp          = DateTime.Now,
                SubscriptionRemainingTime = TimeSpan.FromMinutes(0)
            }
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
