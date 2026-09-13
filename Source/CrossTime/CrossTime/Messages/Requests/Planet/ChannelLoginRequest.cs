using Encore.Messaging;

namespace CrossTime.Messages.Requests;

public class ChannelLoginRequest : IMessage
{
    public static Enum Command => RequestCommand.ChannelLogin;

    [MessageField(order: 0)]
    public short ServerId { get; init; }

    [MessageField(order: 1)]
    public short ChannelId { get; init; }
}
