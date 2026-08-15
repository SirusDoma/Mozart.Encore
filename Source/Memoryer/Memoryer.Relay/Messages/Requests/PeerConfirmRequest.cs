using System.Net;
using Encore.Messaging;
using Memoryer.Relay.Messages.Codecs;

namespace Memoryer.Relay.Messages.Requests;

public class PeerConfirmRequest : IMessage
{
    public static Enum Command => RelayCommand.PeerConfirm;

    [MessageField(order: 0)]
    private int Code { get; init; }

    [CollectionMessageField<IPEndpointCodec>(order: 1, prefixSizeType: TypeCode.Empty, minCount: 3, maxCount: 3)]
    public IReadOnlyList<IPEndPoint> RelayEndpoints { get; init; } = [];

    [MessageField(order: 2)]
    private ushort Unused { get; init; }

    [MessageField(order: 3)]
    public int SessionKey1 { get; init; }

    [MessageField(order: 4)]
    public int SessionKey2 { get; init; }

    [MessageField(order: 5)]
    public required ConnectionEndpoint LocalEndpoint { get; init; }

    [MessageField(order: 6)]
    public required ConnectionEndpoint PublicEndpoint { get; init; }
}
