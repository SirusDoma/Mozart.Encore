using Encore.Messaging;

namespace Mozart.Messages.Responses;

public class ChannelLogoutResponse : IMessage
{
    public static Enum Command => ResponseCommand.ChannelLogout;

    [MessageField(order: 0)]
    private int Cancel => 0;
}
