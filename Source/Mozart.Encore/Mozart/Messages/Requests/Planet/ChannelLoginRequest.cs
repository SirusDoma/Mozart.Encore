using Encore.Messaging;

namespace Mozart;

public class ChannelLoginRequest : IMessage
{
    public static Enum Command => RequestCommand.ChannelLogin;

    [MessageField(order: 0)]
    public int ChannelId { get; private set; }
}