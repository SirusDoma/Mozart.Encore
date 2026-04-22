using Encore.Messaging;
using Mozart.Metadata;

namespace Memoryer.Messages.Events;

public class WaitingRoomTitleEventData : IMessage
{
    public static Enum Command => EventCommand.WaitingTitleChanged;

    [StringMessageField(order: 0, maxLength: 21)]
    public required string CurrentTitle { get; init; }

    [StringMessageField(order: 1, maxLength: 21)]
    public required string Title { get; init; }

    [MessageField(order: 2)]
    public bool HasPassword { get; init; }

    [MessageField(order: 3)]
    public byte MinLevelLimit { get; set; } = 0;

    [MessageField(order: 4)]
    public byte MaxLevelLimit { get; set; } = 0;

    [MessageField(order: 5)]
    public required KeyMode KeyMode { get; init; }

    [MessageField(order: 6)]
    public required GameMode GameMode { get; init; }

    [MessageField(order: 7)]
    public ushort MusicId { get; init; }
}
