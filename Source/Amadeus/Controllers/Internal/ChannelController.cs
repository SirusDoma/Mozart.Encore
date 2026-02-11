using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Encore.Server;

using Mozart.Data.Repositories;
using Mozart.Entities;
using Amadeus.Internal.Requests;
using Mozart.Services;
using Mozart.Sessions;
using Amadeus.Workers.Channels;

namespace Amadeus.Controllers.Internal;

public class ChannelController(
    UserSession session,
    IUserRepository repository,
    IGatewayClient client,
    IIdentityService identityService,
    IChannelService channelService,
    ILogger<GatewayController> logger
    ) : CommandController<UserSession>(session)
{
    [CommandHandler(GatewayCommand.GetChannelStats)]
    public GetChannelStatsResponse GetChannelStats(GetChannelStatsRequest request)
    {
        logger.LogInformation((int)GatewayCommand.GetChannelStats, "Get channel stats");

        var channel = channelService.GetChannels().Single();
        return new GetChannelStatsResponse
        {
            RequestId = request.RequestId,
            Id        = channel.Id,
            Capacity  = channel.Capacity,
            UserCount = channel.UserCount,
            Gem       = channel.GemRates,
            Exp       = channel.ExpRates
        };
    }

    [CommandHandler]
    public async Task<GrantSessionResponse> GrantSession(GrantSessionRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation((int)GatewayCommand.GrantSession, "Grant session");

        IChannel? channel = null;

        try
        {
            channel           = channelService.GetChannels().Single();
            var authSession   = await identityService.Authorize(request.SessionId, cancellationToken);
            var characterInfo = await repository.Find(authSession.UserId, cancellationToken);

            if (characterInfo == null)
                throw new ArgumentException("Invalid session id");

            var session = await client.EnqueueSession(request.SessionId, cancellationToken);
            session.Authorize(new Actor(characterInfo)
            {
                Token = request.SessionId,
                ClientId = request.ClientId
            });

            session.Register(channel);

            return new GrantSessionResponse
            {
                Success   = true,
                SessionId = request.SessionId,
                ClientId  = request.ClientId,
                Username  = characterInfo.Username,
                Nickname  = characterInfo.Nickname,
                Ranking   = characterInfo.Ranking,
                ChannelId = channel.Id,
                Capacity  = channel.Capacity,
                UserCount = channel.UserCount,
            };
        }
        catch (Exception ex)
        {
            logger.LogError((int)GatewayCommand.GrantSession, ex, "Failed to grant session");

            return new GrantSessionResponse
            {
                Success   = false,
                SessionId = request.SessionId,
                ClientId  = request.ClientId,
                Username  = string.Empty,
                Nickname  = string.Empty,
                Ranking   = 0,
                ChannelId = channel?.Id        ?? -1,
                Capacity  = channel?.Capacity  ?? 0,
                UserCount = channel?.UserCount ?? 0,
            };
        }
    }

    [CommandHandler]
    public async Task RevokeSession(RevokeSessionRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation((int)GatewayCommand.RevokeSession, "Revoke session");

        if (Session.Authorized)
        {
            var channel = channelService.GetChannels().Single();
            if (Session.Channel != null)
                Session.Exit(channel);
        }

        await client.RevokeSession(request.SessionId, cancellationToken);
    }

    [CommandHandler]
    public async Task Relay(GatewayRelayRequest request, CancellationToken cancellationToken)
    {
        // Use to relay incoming request: user -> gateway -> channel

        logger.LogTrace((int)GatewayCommand.Relay, "Gateway relay message");
        await client.Dispatch(Session, request.Payload, cancellationToken);
    }
}