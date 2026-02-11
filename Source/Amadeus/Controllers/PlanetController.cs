using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

using Encore.Server;

using Amadeus.Messages.Requests;
using Amadeus.Messages.Responses;
using Mozart.Options;
using Mozart.Services;
using Mozart.Sessions;

namespace Amadeus.Controllers;

[Authorize]
public class PlanetController(Session session, ISessionManager manager, IIdentityService identityService, IChannelService channelService,
    IOptions<GatewayOptions> options, ILogger<PlanetController> logger) : CommandController<Session>(session)
{
    [CommandHandler(RequestCommand.GetChannelList)]
    public ChannelListResponse GetChannelList()
    {
        var gateway = options.Value;
        logger.LogInformation(
            (int)RequestCommand.GetChannelList,
            "Get channel list: [{GatewayId}]",
            gateway.Id
        );

        var states     = new List<ChannelListResponse.ChannelState>();
        var channels   = channelService.GetChannels();

        if (channels.Count > 0)
        {
            int maxChannel = channels.Max(c => c.Id) + 1;
            for (ushort i = 0; i < maxChannel; i++)
            {
                var channel = channels.SingleOrDefault(s => s.Id == i);
                states.Add(new ChannelListResponse.ChannelState
                {
                    ServerId   = (ushort)gateway.Id,
                    ChannelId  = i,
                    Capacity   = channel?.Capacity ?? 0,
                    Population = channel?.UserCount ?? 0,
                    Active     = channel != null
                });
            }
        }

        return new ChannelListResponse { Channels = states };
    }

    [CommandHandler]
    public async Task<ChannelLoginResponse> ChannelLogin(ChannelLoginRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            (int)RequestCommand.ChannelLogin,
            "Enter channel: [{GatewayId}/{ChannelId:00}]",
            request.ServerId,
            request.ChannelId
        );

        if (request.ServerId != options.Value.Id)
            throw new ArgumentOutOfRangeException(nameof(request), "Invalid server id");

        const StringComparison comparison = StringComparison.InvariantCultureIgnoreCase;
        var existingSession = channelService.Sessions.FirstOrDefault(s => s.Actor.Token.Equals(Session.Actor.Token, comparison));
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
                await Session.WriteMessage(new AuthResponse
                {
                    Result = AuthResult.ClientDuplicateSessions,
                    Subscription = new AuthResponse.SubscriptionInfo()
                }, cancellationToken);

                return null!;
            }
        }

        try
        {
            var channel = channelService.GetChannel(request.ChannelId);
            Session.Register(channel);

            await identityService.UpdateChannel(Session.Actor.Token, options.Value.Id, request.ChannelId, cancellationToken);
            return new ChannelLoginResponse
            {
                Failed    = false,
                ErrorCode = LoginErrorCode.Undefined,
                Nickname  = Session.Actor.Nickname,
                Username  = Session.Actor.Username,
                Ranking   = Session.Actor.Ranking
            };
        }
        catch (InvalidOperationException)
        {
            return new ChannelLoginResponse
            {
                Failed    = true,
                ErrorCode = LoginErrorCode.Undefined,
                Nickname  = string.Empty,
                Username  = string.Empty,
                Ranking   = 0
            };
        }
    }
}
