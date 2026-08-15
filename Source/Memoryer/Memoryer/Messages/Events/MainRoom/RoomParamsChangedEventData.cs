using Encore.Messaging;
using Mozart.Metadata;

namespace Memoryer.Messages.Events;

public class RoomParamsChangedEventData : IMessage
{
    public static Enum Command => EventCommand.RoomParamsChanged;

    [MessageField(order: 0)]
    public int Number { get; init; }

    [StringMessageField(order: 1)]
    public required string Title { get; init; }

    [MessageField(order: 2)]
    public required KeyMode KeyMode { get; init; }

    [MessageField(order: 3)]
    public required GameMode GameMode { get; init; }

    [MessageField(order: 4)]
    public bool HasPassword { get; init; }

    [StringMessageField(order: 5, maxLength: 21)]
    public required string Password { get; init; }

    [MessageField(order: 6)]
    public byte MinLevelLimit { get; set; }

    [MessageField(order: 7)]
    public byte MaxLevelLimit { get; set; }

    [MessageField(order: 8)]
    public int MusicId { get; init; }
}
