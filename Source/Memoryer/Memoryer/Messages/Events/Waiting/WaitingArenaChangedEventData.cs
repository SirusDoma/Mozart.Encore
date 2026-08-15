using Encore.Messaging;

namespace Memoryer.Messages.Events;

public class WaitingArenaChangedEventData : IMessage
{
    public static Enum Command => EventCommand.RoomArenaChanged;

    [MessageField(order: 0)]
    public int Arena { get; init; }
}
