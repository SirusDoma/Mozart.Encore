using Encore.Messaging;

namespace Amadeus.Messages.Responses;

public class CreateRoomResponse : IMessage
{
    public enum CreateResult : int
    {
        Success     = 0x00000000, // 0
        ChannelFull = 0x00000001  // 1
    }

    public static Enum Command => ResponseCommand.CreateRoom;

    [MessageField(order: 0)]
    public CreateResult Result = CreateResult.Success;

    [MessageField(order: 1)]
    public int Number { get; init; }
}
