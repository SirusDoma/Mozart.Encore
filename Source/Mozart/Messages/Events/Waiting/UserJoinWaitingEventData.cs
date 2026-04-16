using Encore.Messaging;
using Mozart.Data.Entities;
using Mozart.Metadata;
using Mozart.Metadata.Items;

namespace Mozart.Messages.Events;

public class UserJoinWaitingEventData : IMessage
{
    public static Enum Command => EventCommand.UserJoinWaiting;

    [MessageField(order: 0)]
    public byte MemberId { get; init; }

    [StringMessageField(order: 1)]
    public required string Nickname { get; init; }

    [MessageField(order: 2)]
    public int Level { get; init; }

    [MessageField(order: 3)]
    public Gender Gender { get; init; }

    [MessageField(order: 4)]
    public RoomTeam Team { get; init; }

    [MessageField(order: 5)]
    public bool Ready { get; init; }

    [MessageField<CharacterEquipmentInfoCodec>(order: 6)]
    public Dictionary<ItemType, int> Equipments { get; init; } = [];

    [CollectionMessageField(order: 9, prefixSizeType: TypeCode.Int32)]
    public IReadOnlyList<ushort> MusicIds { get; init; } = [];

    [MessageField(order: 10)]
    public int CashPoint { get; init; }

    [MessageField(order: 11)]
    public FreePassType FreePass { get; init; }

    [MessageField(order: 12)]
    public bool IsPlaying { get; init; }

    [MessageField(order: 13)]
    public bool IsAdministrator { get; init; }
}
