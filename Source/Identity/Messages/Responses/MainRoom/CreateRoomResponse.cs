using Encore.Messaging;

namespace Identity.Messages.Responses;

public class CreateRoomResponse : IMessage
{
    public static Enum Command => ResponseCommand.CreateRoom;

    public enum CreateResult : int
    {
        Success     = 0x00000000, // 0
        ChannelFull = 0x00000001  // 1
    }

    [MessageField(order: 0)]
    public CreateResult Result = CreateResult.Success;

    [MessageField(order: 1)]
    public int Number { get; init; }

    [MessageField<MessageFieldCodec<short>>(order: 2)]
    public bool Premium { get; init; }
}
