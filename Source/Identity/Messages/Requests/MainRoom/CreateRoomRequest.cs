using Encore.Messaging;
using Mozart.Metadata;

namespace Identity.Messages.Requests;

public class CreateRoomRequest : IMessage
{
    public static Enum Command => RequestCommand.CreateRoom;

    [StringMessageField(order: 0, maxLength: 22)]
    public required string Title { get; init; }

    [MessageField(order: 1)]
    public required GameMode Mode { get; init; }

    [MessageField(order: 2)]
    public bool HasPassword { get; init; }

    [StringMessageField(order: 3, maxLength: 12)]
    public required string Password { get; init; }

    [MessageField(order: 4)]
    public byte MinLevelLimit { get; set; } = 0;

    [MessageField(order: 5)]
    public byte MaxLevelLimit { get; set; } = 0;

    [MessageField<MessageFieldCodec<short>>(order: 6)]
    public bool Premium { get; set; }
}
