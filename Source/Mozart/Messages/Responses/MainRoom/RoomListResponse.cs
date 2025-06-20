using Encore.Messaging;
using Mozart.Metadata;

namespace Mozart.Messages.Responses;


public class RoomListResponse : IMessage
{
    public static Enum Command => ResponseCommand.GetRoomList;

    public class RoomInfo : SubMessage
    {
        [MessageField(order: 0)]
        public int Number { get; set; } = 0;

        [MessageField(order: 1)]
        public RoomState State { get; set; }

        [StringMessageField(order: 2, maxLength: 21)]
        public string Title { get; init; } = string.Empty;

        [MessageField(order: 3)]
        public bool HasPassword { get; set; } = false;

        [MessageField(order: 4)]
        public int MusicId { get; init; } = 0;

        [MessageField(order: 5)]
        public Difficulty Difficulty { get; set; } = Difficulty.EX;

        [MessageField(order: 6)]
        public GameMode Mode { get; set; } = GameMode.Single;

        [MessageField(order: 7)]
        public GameSpeed Speed { get; set; } = GameSpeed.X10;

        [MessageField(order: 8)]
        public byte Capacity { get; set; } = 8;

        [MessageField(order: 9)]
        public byte UserCount { get; set; } = 0;

        [MessageField(order: 10)]
        public byte MinLevelLimit { get; set; } = 0;

        [MessageField(order: 11)]
        public byte MaxLevelLimit { get; set; } = 0;
    }

    [CollectionMessageField(order: 0, maxCount: 100, minCount: 100, prefixSizeType: TypeCode.Int32)]
    public required IReadOnlyList<RoomInfo> Rooms { get; init; }
}