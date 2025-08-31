using Encore.Messaging;
using Mozart.Metadata;

namespace Amadeus.Messages.Events;

public class RoomCreatedEventData : IMessage
{
    public static Enum Command => EventCommand.RoomCreated;

    [MessageField(order: 0)]
    public int Number { get; init; }

    [StringMessageField(order: 1)]
    public required string Title { get; init; }

    [MessageField(order: 2)]
    public GameMode Mode { get; init; } = GameMode.Single;

    [MessageField(order: 3)]
    public bool HasPassword { get; init; } = false;

    [MessageField(order: 4)]
    public byte MinLevelLimit { get; set; } = 0;

    [MessageField(order: 5)]
    public byte MaxLevelLimit { get; set; } = 0;
}