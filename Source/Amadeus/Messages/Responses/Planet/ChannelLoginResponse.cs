using Encore.Messaging;

namespace Amadeus.Messages.Responses;

public enum LoginErrorCode : uint
{
    Undefined   = 0x00000000, // Used for channel full
    PremiumOnly = 0x00000001,
}

public enum ServerType : uint
{
    Default = 0x00000000,
    Premium = 0x00000001,
    Free    = 0x00000002,
}

public class ChannelLoginResponse : IMessage
{
    public static Enum Command => ResponseCommand.ChannelLogin;

    public class FailureInfo : LoginInfo
    {
        [MessageField(order: 0)]
        public LoginErrorCode ErrorCode { get; init; } = LoginErrorCode.Undefined;
    }

    public class SuccessInfo : LoginInfo
    {
        [MessageField(order: 0)]
        public ServerType ServerType { get; init; } = ServerType.Default;
    }

    public abstract class LoginInfo : SubMessage;

    [MessageField<MessageFieldCodec<int>>(order: 0)]
    public bool Failed { get; init; } = false;

    [MessageField(order: 1)]
    public required LoginInfo Info { get; init; }

    [StringMessageField(order: 2)]
    public required string Nickname { get; init; }

    [StringMessageField(order: 3)]
    public required string Username { get; init; }

    [MessageField(order: 4)]
    public int Ranking { get; init; }
}
