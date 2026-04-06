using Encore.Messaging;

namespace Identity.Messages.Responses;

public class ChannelLogoutResponse : IMessage
{
    public static Enum Command => ResponseCommand.ChannelLogout;

    [MessageField(order: 0)]
    private int UnusedFlag => 0; // 1 to "cancel" the logout
}
