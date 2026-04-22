using Encore.Messaging;
using Memoryer.Messages.Codecs;
using Mozart.Data.Entities;
using Mozart.Metadata;
using Mozart.Metadata.Items;

namespace Memoryer.Messages.Events;

public class UserLeaveWaitingEventData : IMessage
{
    public static Enum Command => EventCommand.UserLeaveWaiting;

    public class LiveState : SubMessage
    {
        [MessageField(order: 0)]
        public bool Invalid { get; init; } = false;

        [MessageField(order: 1)]
        public int? UserCount { get; init; }

        [CollectionMessageField(order: 2, minCount: 8, maxCount: 8)]
        public required IReadOnlyList<MemberLiveState>? Members { get; init; }
    }

    public class MemberLiveState : SubMessage
    {
        [MessageField(order: 0)]
        public bool Active { get; init; }

        [MessageField(order: 1)]
        public MemberInfo? MemberInfo { get; init; }
    }

    public class MemberInfo : SubMessage
    {
        [MessageField(order: 0)]
        public byte MemberId { get; init; }

        [StringMessageField(order: 1)]
        public required string Nickname { get; init; }

        [MessageField(order: 2)]
        public int Level { get; init; }

        [MessageField(order: 3)]
        public Gender Gender { get; init; }

        [MessageField(order: 4)]
        public int Gem { get; init; }

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
        public bool IsPlaying { get; init; }

        [MessageField(order: 13)]
        public bool IsAdministrator { get; init; }

        [MessageField(order: 14)]
        public bool IsRoomMaster { get; init; }

        [MessageField(order: 15)]
        public int WinStreak { get; init; }
    }

    [MessageField(order: 0)]
    public byte MemberId { get; init; }

    [MessageField(order: 1)]
    public byte NewRoomMasterMemberId { get; init; }

    [MessageField<MessageFieldCodec<short>>(order: 2)]
    public bool Premium { get; init; }

    [MessageField<MessageFieldCodec<int>>(order: 3)]
    public bool HasSuperRoomManager { get; init; }

    [MessageField(order: 4)]
    public LiveState? LiveMode { get; init; }
}
