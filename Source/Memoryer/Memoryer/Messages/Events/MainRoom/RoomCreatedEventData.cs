using Encore.Messaging;
using Mozart.Metadata;

namespace Memoryer.Messages.Events;

public class RoomCreatedEventData : IMessage
{
    public static Enum Command => EventCommand.RoomCreated;

    [MessageField(order: 0)]
    public int Number { get; init; }

    [StringMessageField(order: 1)]
    public required string Title { get; init; }

    [MessageField(order: 2)]
    public KeyMode KeyMode { get; init; } = KeyMode.SevenKeys;

    [MessageField(order: 3)]
    public GameMode GameMode { get; init; } = GameMode.Normal;

    [MessageField(order: 4)]
    public bool HasPassword { get; init; } = false;

    [MessageField(order: 5)]
    public byte MinLevelLimit { get; set; } = 0;

    [MessageField(order: 6)]
    public byte MaxLevelLimit { get; set; } = 0;

    [MessageField<MessageFieldCodec<short>>(order: 7)]
    public bool Premium { get; set; }

    [MessageField(order: 8)]
    public byte Type { get; set; } = 0;
}
