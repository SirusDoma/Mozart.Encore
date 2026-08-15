using Encore.Messaging;

namespace Memoryer.Messages.Requests;

public class TestNetworkLatencyRequest : IMessage
{
    public static Enum Command => RequestCommand.TestNetworkLatency;

    [MessageField<MessageFieldCodec<int>>(order: 0)]
    public byte MemberId { get; init; }

    [MessageField(order: 1)]
    public int RemoteTick { get; init; }

    [MessageField(order: 2)]
    public int LocalTick { get; init; }

    [MessageField(order: 3)]
    public int Sequence { get; init; }

    [MessageField<MessageFieldCodec<int>>(order: 4)]
    public bool Last { get; init; }
}
