using Encore.Messaging;
using Mozart.Metadata.Room;

namespace Amadeus.Messages.Events
{
    public class RoomSlotUpdateEventData : IMessage
    {
        public static Enum Command => EventCommand.RoomSlotUpdate;

        [MessageField(order: 0)]
        public byte Index { get; init; }

        [MessageField(order: 1)]
        public RoomSlotActionType Type { get; init; }
    }
}
