using Encore.Messaging;
using Mozart.Metadata;

namespace Memoryer.Messages.Requests;

public class CreateRoomRequest : IMessage
{
    public static Enum Command => RequestCommand.CreateRoom;

    [StringMessageField(order: 0, maxLength: 22)]
    public required string Title { get; init; }

    [MessageField(order: 1)]
    public required KeyMode KeyMode { get; init; }

    [MessageField(order: 2)]
    public required GameMode GameMode { get; init; }

    [MessageField(order: 3)]
    public bool HasPassword { get; init; }

    [StringMessageField(order: 4, maxLength: 12)]
    public required string Password { get; init; }

    [MessageField(order: 5)]
    public byte MinLevelLimit { get; set; } = 0;

    [MessageField(order: 6)]
    public byte MaxLevelLimit { get; set; } = 0;

    [MessageField<MessageFieldCodec<short>>(order: 7)]
    public bool Premium { get; set; }
}
