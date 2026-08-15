using System.Net;
using Encore.Messaging;
using Memoryer.Messages.Codecs;

namespace Memoryer.Messages.Responses;

public class P2PListResponse : IMessage
{
    public static Enum Command => ResponseCommand.GetP2PList;

    public class PeerInfo : SubMessage
    {
        [MessageField<MessageFieldCodec<int>>(order: 0)]
        public byte MemberId { get; init; }

        [MessageField<IPEndpointCodec>(order: 1)]
        public required IPEndPoint PublicEndpoint { get; init; }

        [MessageField<IPEndpointCodec>(order: 2)]
        public required IPEndPoint LocalEndpoint { get; init; }
    }

    [CollectionMessageField(order: 0, prefixSizeType: TypeCode.Int32)]
    public required IReadOnlyList<PeerInfo> Peers { get; init; }
}
