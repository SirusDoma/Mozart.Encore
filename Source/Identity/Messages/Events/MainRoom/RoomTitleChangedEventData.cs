using Encore.Messaging;
using Mozart.Metadata;

namespace Memoryer.Messages.Events;

public class RoomTitleChangedEventData : IMessage
{
    public static Enum Command => EventCommand.RoomTitleChanged;

    [MessageField(order: 0)]
    public int Number { get; init; }

    [StringMessageField(order: 1)]
    public required string Title { get; init; }

    [MessageField(order: 1)]
    public required KeyMode KeyMode { get; init; }

    [MessageField(order: 2)]
    public required GameMode GameMode { get; init; }

    [MessageField(order: 3)]
    public bool HasPassword { get; init; }

    [MessageField(order: 4)]
    public byte MinLevelLimit { get; set; }

    [MessageField(order: 5)]
    public byte MaxLevelLimit { get; set; }
}
