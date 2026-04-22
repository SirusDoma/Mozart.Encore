using Encore.Messaging;
using Memoryer.Messages.Codecs;
using Mozart.Data.Entities;
using Mozart.Metadata;
using Mozart.Metadata.Items;

namespace Memoryer.Messages.Responses;

public class JoinRoomResponse : IMessage
{
    public static Enum Command => ResponseCommand.JoinRoom;

    public enum JoinResult : uint
    {
        Success         = 0x00000000, // 0
        GenericError    = 0xFFFFFFFF, // -1
        InvalidMode     = 0xFFFFFFFE, // -2
        InvalidPassword = 0xFFFFFFFD, // -3
        InProgress      = 0xFFFFFFFB, // -5
        Full            = 0xFFFFFFFA, // -6 (or -4 / 0xFFFFFFFC)
        NoPass          = 0xFFFFFFF9, // -7
        InvalidNumber   = 0xFFFFFFF8  // -8
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
        public int Gem { get; init; }

        [MessageField(order: 4)]
        public bool IsRoomMaster { get; init; }

        [MessageField(order: 5)]
        public RoomTeam Team { get; init; }

        [MessageField(order: 6)]
        public bool Ready { get; init; }

        [MessageField(order: 7)]
        public MusicState MusicState { get; init; }

        [MessageField<CharacterEquipmentInfoCodec>(order: 8)]
        public Dictionary<ItemType, int> Equipments { get; init; } = [];

        [CollectionMessageField(order: 9, prefixSizeType: TypeCode.Int32)]
        public IReadOnlyList<ushort> MusicIds { get; init; } = [];

        [MessageField(order: 10)]
        public int CashPoint { get; init; }

        [MessageField(order: 11)]
        public FreePassType FreePass { get; init; }

        [MessageField(order: 12)]
        public PlayingState PlayingState { get; init; }

        [MessageField(order: 13)]
        public bool IsAdministrator { get; init; }

        [MessageField<MessageFieldCodec<int>>(order: 14)]
        public bool IsSuperRoomManager { get; init; }
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
    public byte MemberId { get; init; }

    [MessageField(order: 2)]
    public RoomTeam Team { get; init; }

    [StringMessageField(order: 3)]
    public string RoomTitle { get; init; } = string.Empty;

    [MessageField(order: 4)]
    public ushort MusicId { get; init; }

    [MessageField(order: 5)]
    public RoomArenaMessage ArenaInfo { get; init; } = new();

    [MessageField(order: 6)]
    public KeyMode KeyMode { get; init; }

    [MessageField(order: 7)]
    public GameMode GameMode { get; init; }

    [MessageField(order: 7)]
    public Difficulty Difficulty { get; init; }

    [MessageField(order: 8)]
    public GameSpeed Speed { get; init; }

    [MessageField<MessageFieldCodec<int>>(order: 9)]
    public bool HasSuperRoomManager { get; init; }

    [MessageField(order: 10)]
    public int UserCount { get; init; }

    [CollectionMessageField(order: 11, minCount: 7, maxCount: 7)]
    public IReadOnlyList<RoomSlotInfo> Slots { get; init; } = [];

    [CollectionMessageField(order: 12, prefixSizeType: TypeCode.Int32)]
    public IReadOnlyList<int> Skills { get; init; } = [];

    [MessageField<MessageFieldCodec<short>>(order: 13)]
    public bool Premium { get; init; }

    [MessageField(order: 14)]
    public byte? ChampionMemberId { get; init; } = null;

    [MessageField(order: 15)]
    public int? ChampionWinStreak { get; init; } = null;
}
