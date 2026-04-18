using Encore.Messaging;

namespace Identity.Messages.Responses;

public enum ChannelLoginResult : int
{
    Success                = 0,
    PremiumNotAuthorized   = 1,
    ChannelInfoUnavailable = 6,
    ChannelUnavailable     = 7,
}

public class ChannelLoginResponse : IMessage
{
    public static Enum Command => ResponseCommand.ChannelLogin;

    [MessageField(order: 0)]
    public ChannelLoginResult Result { get; init; }

    [MessageField<MessageFieldCodec<int>>(order: 1)]
    public bool Restricted { get; init; } = false;
}
