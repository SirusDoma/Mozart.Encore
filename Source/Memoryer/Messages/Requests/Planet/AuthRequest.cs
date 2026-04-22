using System.Net;
using Encore.Messaging;
using Memoryer.Messages.Codecs;
using Mozart.Metadata;

namespace Memoryer.Messages.Requests;

public class AuthRequest : IMessage
{
    public static Enum Command => RequestCommand.Authorize;

    [MessageField(order: 0)]
    private byte Unused1 { get; set; } // Hardcoded to 1

    [MessageField(order: 1)]
    public int RelaySessionKey1 { get; init; }

    [MessageField(order: 2)]
    public int RelaySessionKey2 { get; init; }

    [MessageField<IPEndpointCodec>(order: 3)]
    public required IPEndPoint PublicEndpoint { get; init; }

    [MessageField<IPEndpointCodec>(order: 4)]
    public required IPEndPoint LocalEndpoint { get; init; }

    [MessageField(order: 5)]
    public int UserId { get; init; }

    [StringMessageField(order: 6)]
    public string Token { get; init; } = string.Empty; // Username

    [MessageField<AuthGenderCodec>(order: 7)]
    public Gender Gender { get; init; } = Gender.Any;

    [MessageField(order: 8)]
    private byte Unused2 { get; init; } // Hardcoded to 1

    [StringMessageField(order: 9)]
    public string ClientId { get; init; } = string.Empty;
}
