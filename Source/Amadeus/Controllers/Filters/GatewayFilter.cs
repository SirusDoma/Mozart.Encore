using Encore.Messaging;
using Encore.Server;
using Microsoft.Extensions.Options;
using Amadeus.Internal.Requests;
using Amadeus.Messages.Requests;
using Amadeus.Messages.Responses;
using Mozart.Options;
using Amadeus.Workers.Gateway;

namespace Amadeus.Controllers.Filters;

public class GatewayFilter(IOptions<GatewayOptions> options, IChannelAggregator aggregator,
    IChannelSessionManager channelManager, IMessageCodec codec) : CommandFilter
{
    public override async Task OnActionExecutingAsync(CommandExecutingContext context, CancellationToken cancellationToken = default)
    {
        if (context.Session is not ClientSession session)
            return;

        var command = (RequestCommand)context.Command;
        if (command == RequestCommand.Terminate)
        {
            session.Terminate();
            return;
        }

        if (!session.Authorized)
        {
            if (command != RequestCommand.Authorize)
                throw new InvalidOperationException("Unauthorized");

            return;
        }

        if (context.Command.Equals(RequestCommand.GetChannelList))
        {
            context.Cancel = true;

            var channels = channelManager.GetChannelSessions();
            if (channels.Count > 0)
            {
                string requestId = aggregator.Create(session, channels.Count, async Task (result) =>
                {
                    var acquiredStats = result.AcquiredStats;
                    var responses = new List<ChannelListResponse.ChannelState>();

                    int maxChannel = acquiredStats.Max(c => c.ChannelId) + 1;
                    for (ushort i = 0; i < maxChannel; i++)
                    {
                        var channel = acquiredStats.SingleOrDefault(s => s.ChannelId == i);
                        responses.Add(new ChannelListResponse.ChannelState
                        {
                            ServerId   = (ushort)(channel != null ? options.Value.Id : 0),
                            ChannelId  = i,
                            Capacity   = channel?.Capacity  ?? 0,
                            Population = channel?.UserCount ?? 0,
                            Active     = channel != null
                        });
                    }

                    await result.Session.WriteFrame(codec.Encode(new ChannelListResponse()
                    {
                        Channels = responses                   }), cancellationToken);
                });

                session.Properties["ChannelStatsAggregatorRequestId"] = requestId;
                foreach (var channel in channels)
                    await channel.RequestChannelStats(requestId, cancellationToken);
            }
        }
        if (context.Command.Equals(RequestCommand.ChannelLogin))
        {
            context.Cancel = true;

            var request = (ChannelLoginRequest)context.Request!;
            var channel = channelManager.GetChannelSession(request.ChannelId);

            if (channel == null)
                throw new InvalidOperationException("Invalid channel id");

            if (!session.HasChannelSession)
                await channel.Register(session, cancellationToken);
        }
        else if (session.ChannelId != null)
        {
            context.Cancel = true;

            var channel = channelManager.GetChannelSession(session.ChannelId.Value);
            if (channel == null)
                throw new InvalidOperationException("Invalid channel id");

            var result = new GatewayRelayRequest
            {
                SessionId = session.Id,
                Payload   = context.Request != null
                    ? codec.Encode(context.Request)
                    : codec.EncodeCommand(context.Command)
            };

            await channel.WriteFrame(codec.Encode(result), cancellationToken);
        }
    }
}
