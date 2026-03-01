using System.Diagnostics.CodeAnalysis;
using CrossTime.Messages.Codecs;
using Encore.Messaging;

namespace CrossTime.Messages.Responses;

public class ServerLoginResponse : IMessage
{

    public enum LoginResult : uint
    {
        Success           = 0x00000000,
        Forbidden         = 0xFFFFFFFC, // -4
        InvalidParameter  = 0xFFFFFFFF, // -1
    }

    public static Enum Command => ResponseCommand.ServerLogin;

    [MessageField(order: 0)]
    public LoginResult Result { get; init; }
}
