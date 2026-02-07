using Encore.Messaging;

namespace Amadeus.Messages.Responses;

public enum LoginErrorCode : uint
{
    Undefined   = 0x00000000, // Used for channel full
    PremiumOnly = 0x00000001,
}

public class ChannelLoginResponse : IMessage
{
    public static Enum Command => ResponseCommand.ChannelLogin;

    [MessageField<MessageFieldCodec<int>>(order: 0)]
    public bool Failed { get; init; } = false;

    [MessageField(order: 1)]
    public LoginErrorCode ErrorCode { get; init; } = LoginErrorCode.Undefined;

    [StringMessageField(order: 2)]
    public required string Nickname { get; init; }

    [StringMessageField(order: 3)]
    public required string Username { get; init; }

    [MessageField(order: 4)]
    public int Rank { get; init; }
}