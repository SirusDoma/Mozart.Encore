using Encore.Messaging;
using Encore.Metadata;

namespace Encore.Messages.Responses;

public class CreateSessionResponse : IMessage
{
    public static Enum Command => GameManagerCommand.GrantSession;

    [MessageField(order: 0)]
    public int Result { get; init; }

    [MessageField(order: 1)]
    public uint SessionId { get; init; }

    // Only present when Result == 0
    [MessageField(order: 2)]
    public CharacterInfo? Character { get; init; }

    public class CharacterInfo : SubMessage
    {
        [MessageField(order: 0)]
        public int UserId { get; init; }

        [MessageField(order: 1)]
        public ushort GatewayId { get; init; }

        [MessageField(order: 2)]
        public ushort ChannelId { get; init; }

        [StringMessageField(order: 3)]
        public string Username { get; init; } = string.Empty;

        [StringMessageField(order: 4)]
        public string Nickname { get; init; } = string.Empty;

        [MessageField(order: 5)]
        public Gender Gender { get; init; }

        [MessageField(order: 6)]
        public bool IsAdministrator { get; init; }

        [MessageField(order: 7)]
        public int Gem { get; init; }

        [MessageField(order: 8)]
        public int Point { get; init; }

        [MessageField(order: 9)]
        public int O2Cash { get; init; }

        [MessageField(order: 10)]
        public int Level { get; init; }

        [MessageField(order: 11)]
        public int Battle { get; init; }

        [MessageField(order: 12)]
        public int Win { get; init; }

        [MessageField(order: 13)]
        public int Draw { get; init; }

        [MessageField(order: 14)]
        public int Lose { get; init; }

        [MessageField(order: 15)]
        public int Experience { get; init; }

        [MessageField(order: 16)]
        public int BonusPoint { get; init; }

        [MessageField(order: 17)]
        public int UnreadGiftMessages { get; init; }

        [CollectionMessageField(order: 18, minCount: 16, maxCount: 16)]
        public IReadOnlyList<int> Equipments { get; init; } = [];

        [CollectionMessageField(order: 19, minCount: 30, maxCount: 30)]
        public IReadOnlyList<int> Inventory { get; init; } = [];

        [CollectionMessageField(order: 20, prefixSizeType: TypeCode.Int32)]
        public IReadOnlyList<AttributiveItemEntry> AttributiveItems { get; init; } = [];

        [CollectionMessageField(order: 21, prefixSizeType: TypeCode.Int32)]
        public IReadOnlyList<int> MusicIds { get; init; } = [];

        [CollectionMessageField(order: 22, prefixSizeType: TypeCode.Int32)]
        public IReadOnlyList<int> AcquiredMusicIds { get; init; } = [];

        [CollectionMessageField(order: 23, prefixSizeType: TypeCode.Int32)]
        public IReadOnlyList<GiftItemEntry> ItemGiftBox { get; init; } = [];

        [CollectionMessageField(order: 24, prefixSizeType: TypeCode.Int32)]
        public IReadOnlyList<GiftMusicEntry> MusicGiftBox { get; init; } = [];

        [MessageField(order: 25)]
        public int MusicCash { get; init; }

        [MessageField(order: 26)]
        public int ItemCash { get; init; }

        [MessageField(order: 27)]
        public int GameCount { get; init; }
    }

    public class AttributiveItemEntry : SubMessage
    {
        [MessageField(order: 0)]
        public int ItemId { get; init; }

        [MessageField(order: 1)]
        public int Count { get; init; }
    }

    public class GiftItemEntry : SubMessage
    {
        [MessageField(order: 0)]
        public int GiftId { get; init; }

        [MessageField(order: 1)]
        public int ItemId { get; init; }

        [StringMessageField(order: 2)]
        public string SenderNickname { get; init; } = string.Empty;
    }

    public class GiftMusicEntry : SubMessage
    {
        [MessageField(order: 0)]
        public int GiftId { get; init; }

        [MessageField(order: 1)]
        public int MusicId { get; init; }

        [StringMessageField(order: 2)]
        public string SenderNickname { get; init; } = string.Empty;
    }

}
