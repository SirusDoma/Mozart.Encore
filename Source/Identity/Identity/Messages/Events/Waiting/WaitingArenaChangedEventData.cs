using Encore.Messaging;

namespace Identity.Messages.Responses;

public class WaitingArenaChangedEventData : IMessage
{
    public static Enum Command => EventCommand.RoomArenaChanged;

    [MessageField(order: 0)]
    public int Arena { get; init; }
}
