using Encore.Messaging;

namespace Encore.Messages.Responses;

public class UserRouteResponse : IMessage
{
    public static Enum Command => GameManagerCommand.UserRoute;

    [MessageField(order: 0)]
    public int Result { get; init; }

    [MessageField(order: 1)]
    public uint SessionId { get; init; }

    [MessageField(order: 2)]
    public int UserId { get; init; }

    [MessageField(order: 3)]
    public ushort GatewayId { get; init; }

    [MessageField(order: 4)]
    public ushort ChannelId { get; init; }
}
