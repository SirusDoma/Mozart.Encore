using Encore.Messaging;
using Mozart.Metadata;

namespace CrossTime.Messages.Requests;

public class SetRoomModeRequest : IMessage
{
    public static Enum Command => RequestCommand.SetRoomMode;

    [MessageField(order: 0)]
    public int Number { get; init; }

    [StringMessageField(order: 1)]
    public required string Title { get; init; }

    [MessageField(order: 2)]
    public GameMode Mode { get; init; } = GameMode.Versus;

    [MessageField(order: 3)]
    public byte HasPassword { get; init; } // Be aware that this field is not reliable

    [StringMessageField(order: 4)]
    public required string Password { get; init; }

    [MessageField(order: 5)]
    private int Padding { get; init; }
}
