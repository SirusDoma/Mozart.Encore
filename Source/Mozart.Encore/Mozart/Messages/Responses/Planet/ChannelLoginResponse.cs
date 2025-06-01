using Encore.Messaging;

namespace Mozart;

public class ChannelLoginResponse : IMessage
{
    public static Enum Command => ResponseCommand.ChannelLogin;

    [MessageField<MessageFieldCodec<int>>(order: 0)]
    public bool Full { get; init; } = false;
}