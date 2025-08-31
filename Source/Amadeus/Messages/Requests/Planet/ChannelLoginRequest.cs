using Encore.Messaging;

namespace Amadeus.Messages.Requests;

public class ChannelLoginRequest : IMessage
{
    public static Enum Command => RequestCommand.ChannelLogin;

    [MessageField(order: 0)]
    public short ServerId { get; private set; }

    [MessageField(order: 1)]
    public short ChannelId { get; private set; }
}