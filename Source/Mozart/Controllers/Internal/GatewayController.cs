using Encore.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Encore.Server;

using Mozart.Internal.Requests;
using Mozart.Messages.Responses;
using Mozart.Options;
using Mozart.Workers.Gateway;

namespace Mozart.Controllers.Internal;

public class GatewayController(ChannelSession session, IChannelAggregator aggregator, IOptions<GatewayOptions> options,
     IMessageCodec codec, ILogger<GatewayController> logger) : CommandController<ChannelSession>(session)
{
    [CommandHandler]
    public CreateChannelResponse CreateChannel(CreateChannelRequest request)
    {
        logger.LogInformation((int)ChannelCommand.CreateChannel, "Create channel [{SID}/{CID:00}]",
            request.GatewayId, request.ChannelId);

        try
        {
            if (options.Value.Id != request.GatewayId)
            {
                throw new InvalidOperationException(
                    $"Gateway Id mismatch (Expected: {options.Value.Id} / Actual: {request.GatewayId})");
            }

            aggregator.Track(request.ChannelId);
            Session.Authorize(request.GatewayId, request.ChannelId);

            return new CreateChannelResponse
            {
                Success = true,
            };
        }
        catch (Exception ex)
        {
            logger.LogError((int)ChannelCommand.CreateChannel, ex, "Failed to create channel");

            return new CreateChannelResponse
            {
                Success = false
            };
        }
    }

    [CommandHandler]
    [Authorize]
    public async Task GrantSession(GrantSessionResponse response, CancellationToken cancellationToken)
    {
        logger.LogInformation((int)ChannelCommand.GrantSession, "Grant session");

        try
        {
            var client = Session.GetClientSessions().Single(c => c.Id == response.SessionId);
            client.Register(Session);

            await client.WriteFrame(codec.Encode(new ChannelLoginResponse
            {
                Full = !response.Success
            }), cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError((int)ChannelCommand.GrantSession, ex, "Failed to process grant session");
        }
    }

    [CommandHandler]
    [Authorize]
    public async Task GetChannelStats(GetChannelStatsResponse response, CancellationToken cancellationToken)
    {
        logger.LogInformation((int)ChannelCommand.GetChannelStats, "Get channel stats");

        try
        {
            bool acquired = aggregator.Add(response.RequestId, new ChannelStats()
            {
                ChannelId = response.Id,
                Capacity  = response.Capacity,
                UserCount = response.UserCount
            });

            if (acquired)
            {
                await aggregator.Acquire(response.RequestId);

                // var result   = aggregator.Acquire(response.RequestId);
                // var channels = result.AcquiredStats;
                // var states   = new List<ChannelListResponse.ChannelState>();
                //
                // int maxChannel = channels.Max(c => c.ChannelId) + 1;
                // for (ushort i = 0; i < maxChannel; i++)
                // {
                //     var channel = channels.SingleOrDefault(s => s.ChannelId == i);
                //     if (channel != null)
                //         await aggregator.Update(channel.ChannelId, channel.Capacity, channel.UserCount);
                //
                //     states.Add(new ChannelListResponse.ChannelState
                //     {
                //         ServerId   = (ushort)(channel != null ? options.Value.Id : 0),
                //         ChannelId  = i,
                //         Capacity   = channel?.Capacity  ?? 0,
                //         Population = channel?.UserCount ?? 0,
                //         Active     = channel != null
                //     });
                // }
                //
                // await result.Session.WriteFrame(codec.Encode(new ChannelListResponse()
                // {
                //     Channels = states
                // }), cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError((int)ChannelCommand.GetChannelStats, ex, "Failed to get channel stats");
        }
    }

    [CommandHandler(ChannelCommand.Relay)]
    [Authorize]
    public async Task Relay(ChannelRelayRequest request, CancellationToken cancellationToken)
    {
        // Used to relay outcoming request:  user <- gateway <- channel

        logger.LogTrace((int)ChannelCommand.Relay, "Channel relay message");
        var client = Session.GetClientSessions().Single(c => c.Id == request.SessionId);

        await client.WriteFrame(request.Payload, cancellationToken);
    }
}
