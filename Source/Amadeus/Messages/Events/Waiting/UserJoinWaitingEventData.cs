using Encore.Messaging;
using Amadeus.Messages.Codecs;
using Mozart.Metadata;
using Mozart.Metadata.Items;

namespace Amadeus.Messages.Events;

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

    [MessageField(order: 6)]
    public byte Unknown1 { get; init; } // is admin?

    [MessageField<CharacterEquipmentInfoCodec>(order: 7)]
    public Dictionary<ItemType, int> Equipments { get; init; } = [];

    [CollectionMessageField(order: 8, prefixSizeType: TypeCode.Int32)]
    public IReadOnlyList<ushort> MusicIds { get; init; } = [];
}