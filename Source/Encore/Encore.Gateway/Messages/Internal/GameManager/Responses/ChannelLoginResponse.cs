using Encore.Messaging;

namespace Encore.Messages.Responses;

public class ChannelLoginResponse : IMessage
{
    public static Enum Command => GameManagerCommand.ChannelLogin;

    [MessageField(order: 0)]
    public int Result { get; init; }

    [MessageField(order: 1)]
    public uint SessionId { get; init; }

    [MessageField(order: 2)]
    public ushort GatewayId { get; init; }

    [MessageField(order: 3)]
    public ushort ChannelId { get; init; }

    [MessageField(order: 4)]
    public int GameCount { get; init; }
}
