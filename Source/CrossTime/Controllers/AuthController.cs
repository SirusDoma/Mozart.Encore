using System.Security.Cryptography;
using CrossTime.Messages.Requests;
using CrossTime.Messages.Responses;
using Encore.Server;
using Microsoft.Extensions.Logging;
using Mozart.Data.Repositories;
using Mozart.Services;
using Mozart.Sessions;
using Session = Mozart.Sessions.Session;

namespace CrossTime.Controllers;

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
    public async Task<ServerLoginResponse> ServerLogin(ServerLoginRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation((int)RequestCommand.ServerLogin,
            "Server Login: [{ServerId}]", request.ServerId);

        if (!Session.Authorized)
        {
            await Authorize(new AuthRequest
            {
                Token = $"{request.Token}|X2"
            }, cancellationToken);
        }

        Session.Actor.ServerId = request.ServerId;
        return new ServerLoginResponse
        {
            Result = Session.Authorized && Session.Actor.Token == request.Token ?
                ServerLoginResponse.LoginResult.Success : ServerLoginResponse.LoginResult.InvalidParameter
        };
    }

    [CommandHandler(RequestCommand.ConnectGateway, ResponseCommand.ConnectGateway)]
    public void ConnectGateway()
    {
        // Response command is actually not handled by the game, this packet act as a ping.
        // Note that if there's no valid music list for the server, the game will say it failed to connect.
        logger.LogInformation((int)RequestCommand.ConnectGateway,
            "Authorize connection to the gateway server");
    }

    [CommandHandler(RequestCommand.Authorize)]
    public async Task<AuthResponse> Authorize(AuthRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation((int)RequestCommand.Authorize,
                "Authorize session (Token: {token})", request.Token);

            string[] tokens = request.Token.Split('|');
            if (tokens.Length != 2)
            {
                return new AuthResponse
                {
                    Result = AuthSessionResult.InvalidParameter
                };
            }

            string authToken = tokens[0];
            string hashToken = tokens[1];

            const StringComparison comparison = StringComparison.InvariantCultureIgnoreCase;
            var existingSession = channelService.Sessions.FirstOrDefault(s => s.Actor.Token.Equals(authToken, comparison));
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
                        Result = AuthSessionResult.DuplicateSessions
                    };
                }
            }

            var authSession = await authService.Authorize(authToken, cancellationToken);
            var characterInfo = await userRepository.Find(authSession.UserId, cancellationToken);

            if (characterInfo == null)
            {
                return new AuthResponse
                {
                    Result = AuthSessionResult.DatabaseError
                };
            }

            Session.Authorize(new Actor(characterInfo)
            {
                Token    = authSession.Token,
                ClientId = hashToken // Not confirmed, but there's no hardcoded client id unlike previous versions..
            });
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning((int)RequestCommand.Authorize, ex, "Failed to authorize session keys [{token}]", request.Token);
            return new AuthResponse
            {
                Result = AuthSessionResult.DuplicateSessions
            };
        }

        manager.CancelExpiry(Session);
        return new AuthResponse
        {
            Result         = AuthSessionResult.Success,
            Id             = Session.Actor.UserId,
            Username       = Session.Actor.Username,
            GemStar        = Session.Actor.GemStar,
            MembershipType = Session.Actor.MembershipType,
            Nickname       = Session.Actor.Nickname,
            Unknown1       = Session.Actor.Gem,
            Unknown2       = Session.Actor.Point,
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
