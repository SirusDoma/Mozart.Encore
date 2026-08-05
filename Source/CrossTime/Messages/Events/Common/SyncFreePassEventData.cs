using Encore.Messaging;
using Mozart.Data.Entities;

namespace CrossTime.Messages.Events;

public class SyncFreePassEventData : IMessage
{
    public static Enum Command => EventCommand.SyncFreePass;

    [MessageField(order: 0)]
    public int Gem { get; init; }

    [MessageField(order: 1)]
    public int Point { get; init; }

    [MessageField(order: 2)]
    public int O2Cash { get; init; }

    [MessageField(order: 3)]
    public int ItemCash { get; init; }

    [MessageField(order: 4)]
    public int MusicCash { get; init; }

    [MessageField(order: 5)]
    public FreePassType FreePass { get; init; } = FreePassType.None;
}
