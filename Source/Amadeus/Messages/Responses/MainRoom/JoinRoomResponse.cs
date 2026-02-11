using Encore.Messaging;
using Amadeus.Messages.Codecs;
using Mozart.Metadata;
using Mozart.Metadata.Items;

namespace Amadeus.Messages.Responses;

public class JoinRoomResponse : IMessage
{
    public static Enum Command => ResponseCommand.JoinRoom;

    public enum JoinResult : uint
    {
        Success         = 0x00000000, // 0
        ConnectionError = 0xFFFFFFFF, // -1
        InvalidMode     = 0xFFFFFFFE, // -2
        InvalidPassword = 0xFFFFFFFD, // -3
        InProgress      = 0xFFFFFFFB, // -5
        Full            = 0xFFFFFFFA  // -6 (or -4 / 0xFFFFFFFC)
    }

    public enum RoomSlotState : int
    {
        Unoccupied = 0x00000000, // 0
        Occupied   = 0x00000001, // 1
        Locked     = 0x00000002, // 2
    }

    public class RoomMemberInfo : SubMessage
    {
        [StringMessageField(order: 0)]
        public required string Nickname { get; init; }

        [MessageField(order: 1)]
        public int Level { get; init; }

        [MessageField(order: 2)]
        public Gender Gender { get; init; }

        [MessageField(order: 3)]
        public bool IsRoomMaster { get; init; }

        [MessageField(order: 4)]
        public RoomTeam Team { get; init; }

        [MessageField(order: 5)]
        public bool Ready { get; init; }

        [MessageField(order: 6)]
        public bool IsAdministrator { get; init; }

        [MessageField<CharacterEquipmentInfoCodec>(order: 7)]
        public Dictionary<ItemType, int> Equipments { get; init; } = [];

        [CollectionMessageField(order: 8, prefixSizeType: TypeCode.Int32)]
        public IReadOnlyList<ushort> MusicIds { get; init; } = [];
    }

    public class RoomSlotInfo : SubMessage
    {
        [MessageField(order: 0)]
        public byte Index { get; init; }

        [MessageField(order: 1)]
        public RoomSlotState State { get; init; }

        [MessageField(order: 2)]
        public RoomMemberInfo? MemberInfo { get; init; } = null;
    }

    [MessageField(order: 0)]
    public JoinResult Result = JoinResult.Success;

    [MessageField(order: 1)]
    public byte Index { get; init; }

    [MessageField(order: 2)]
    public RoomTeam Team { get; init; }

    [StringMessageField(order: 3)]
    public string RoomTitle { get; init; } = string.Empty;

    [MessageField(order: 4)]
    public ushort MusicId { get; init; }

    [MessageField(order: 5)]
    public RoomArenaMessage ArenaInfo { get; init; } = new();

    [MessageField(order: 6)]
    public GameMode Mode { get; init; }

    [MessageField(order: 7)]
    public Difficulty Difficulty { get; init; }

    [MessageField(order: 8)]
    public GameSpeed Speed { get; init; }

    [MessageField(order: 9)]
    public int UserCount { get; init; }

    [CollectionMessageField(order: 10, minCount: 8, maxCount: 8)]
    public IReadOnlyList<RoomSlotInfo> Slots { get; init; } = [];

    [CollectionMessageField(order: 11, prefixSizeType: TypeCode.Int32)]
    public IReadOnlyList<int> Skills { get; init; } = [];
}