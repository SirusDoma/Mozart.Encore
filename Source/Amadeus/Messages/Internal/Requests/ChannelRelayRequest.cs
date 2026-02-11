using Encore.Messaging;

namespace Amadeus.Internal.Requests;

public class CreateChannelRequest : IMessage
{
    public static Enum Command => ChannelCommand.CreateChannel;

    [MessageField(order: 0)]
    public required int GatewayId { get; init; }

    [MessageField(order: 1)]
    public required int ChannelId { get; init; }
}
