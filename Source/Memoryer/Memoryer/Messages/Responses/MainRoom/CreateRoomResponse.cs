using Encore.Messaging;

namespace Memoryer.Messages.Responses;

public class CreateRoomResponse : IMessage
{
    public static Enum Command => ResponseCommand.CreateRoom;

    public enum CreateResult : int
    {
        Success     = 0x00000000, // 0
        ChannelFull = 0x00000001  // 1
    }

    public class RoomInfo : SubMessage
    {
        [MessageField(order: 0)]
        public int Number { get; init; }

        [MessageField<MessageFieldCodec<short>>(order: 1)]
        public bool Premium { get; init; }

        [StringMessageField(order: 2)]
        public string Password { get; init; } = string.Empty;

        [MessageField(order: 3)]
        public bool HasPassword { get; init; }

        [MessageField(order: 4)]
        public byte MinLevelLimit { get; init; }

        [MessageField(order: 5)]
        public byte MaxLevelLimit { get; init; }
    }

    [MessageField(order: 0)]
    public CreateResult Result = CreateResult.Success;

    [MessageField(order: 1)]
    public RoomInfo? Info { get; init; }
}
