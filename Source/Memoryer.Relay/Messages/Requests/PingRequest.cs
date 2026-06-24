using System.Net;
using Encore.Messaging;
using Memoryer.Relay.Messages.Codecs;

namespace Memoryer.Relay.Messages.Requests;

public class PingRequest : IMessage
{
    public static Enum Command => RelayCommand.Ping;

    [MessageField(order: 0)]
    private int Code { get; init; }

    [CollectionMessageField<IPEndpointCodec>(order: 1, prefixSizeType: TypeCode.Empty, minCount: 3, maxCount: 3)]
    public IReadOnlyList<IPEndPoint> RelayEndpoints { get; init; } = [];

    [MessageField(order: 2)]
    private ushort Unused { get; init; }

    [MessageField(order: 3)]
    public uint Start { get; init; }

    [MessageField(order: 4)]
    public uint Tick { get; init; }
}
