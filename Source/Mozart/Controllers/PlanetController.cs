using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

using Encore.Server;

using Mozart.Messages.Requests;
using Mozart.Messages.Responses;
using Mozart.Options;
using Mozart.Services;
using Mozart.Sessions;

namespace Mozart.Controllers;

[Authorize]
public class PlanetController(Session session, IIdentityService identityService, IChannelService channelService,
    IOptions<GatewayOptions> options, ILogger<MessagingController> logger) : CommandController<Session>(session)
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
        if (channelService.Sessions.Any(s => s.Actor.Token.Equals(Session.Actor.Token, comparison)))
        {
            await Session.WriteMessage(new AuthResponse
            {
                Result = AuthResult.DuplicateSessions,
                Subscription = new AuthResponse.SubscriptionInfo()
            }, cancellationToken);

            return null!;
        }

        try
        {
            var channel = channelService.GetChannel(request.ChannelId);
            Session.Register(channel);

            await identityService.UpdateChannel(Session.Actor.Token, options.Value.Id, request.ChannelId, cancellationToken);
            return new ChannelLoginResponse { Full = false };
        }
        catch (InvalidOperationException)
        {
            return new ChannelLoginResponse { Full = true };
        }
    }
}