using Encore.Messaging;

namespace Identity.Messages.Responses;

public enum ChannelLoginResult : uint
{
    Success                = 0x00000000, // 0
    PremiumNotAuthorized   = 0x00000001, // 1
    ChannelInfoUnavailable = 0x00000006, // 6
    ChannelUnavailable     = 0x00000007  // 7
}

public class ChannelLoginResponse : IMessage
{
    public static Enum Command => ResponseCommand.ChannelLogin;

    [MessageField(order: 0)]
    public ChannelLoginResult Result { get; init; }

    [MessageField<MessageFieldCodec<int>>(order: 1)]
    public bool Restricted { get; init; } = false;
}
