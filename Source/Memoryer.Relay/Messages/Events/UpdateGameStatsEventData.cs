using System.Net;
using Encore.Messaging;
using Memoryer.Relay.Messages.Codecs;
using Mozart.Metadata;

namespace Memoryer.Relay.Messages.Events;

public class UpdateGameStatsEventData : IMessage
{
    public static Enum Command => RelayCommand.UpdateGameStats;

    [MessageField(order: 0)]
    private int Code { get; init; }

    [CollectionMessageField<IPEndpointCodec>(order: 1, prefixSizeType: TypeCode.Empty, minCount: 3, maxCount: 3)]
    public IReadOnlyList<IPEndPoint> RelayEndpoints { get; init; } = [];

    [MessageField(order: 2)]
    private ushort Unused { get; init; }

    [MessageField<MessageFieldCodec<ushort>>(order: 3)]
    public RoomLiveRole Role { get; init; }

    [MessageField(order: 4)]
    public ushort Cool { get; init; }

    [MessageField(order: 5)]
    public ushort Good { get; init; }

    [MessageField(order: 6)]
    public ushort Bad { get; init; }

    [MessageField(order: 7)]
    public ushort Miss { get; init; }

    [MessageField(order: 8)]
    public ushort Combo { get; init; }

    [MessageField(order: 9)]
    public ushort MaxCombo { get; init; }

    [MessageField(order: 10)]
    public ushort JamCombo { get; init; }

    [MessageField(order: 11)]
    public ushort TotalJamCombo { get; init; }

    [MessageField(order: 12)]
    public ushort LongNoteCombo { get; init; }

    [MessageField(order: 13)]
    public uint LongNoteScore { get; init; }

    [MessageField(order: 14)]
    public ushort Life { get; init; }

    [MessageField(order: 15)]
    public uint PillBuildup { get; init; }

    [MessageField(order: 16)]
    public uint PillCount { get; init; }

    [MessageField(order: 17)]
    public uint Score { get; init; }
}
