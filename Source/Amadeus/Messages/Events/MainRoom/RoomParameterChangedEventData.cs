using Encore.Messaging;
using Mozart.Metadata;

namespace Amadeus.Messages.Events;

public class RoomParameterChangedEventData : IMessage
{
    public static Enum Command => EventCommand.RoomParameterChanged;

    public abstract class RoomParameter : SubMessage;

    public class MusicParameter : RoomParameter
    {
        [MessageField(order: 0)]
        public ushort MusicId { get; init; }

        [MessageField(order: 1)]
        public Difficulty Difficulty { get; init; }

        [MessageField(order: 2)]
        public GameSpeed Speed { get; init; }
    }

    public class AlbumParameter : RoomParameter
    {
        [MessageField(order: 0)]
        public ushort AlbumId { get; init; }

        [MessageField(order: 1)]
        private byte Unused => 0;

        [MessageField(order: 2)]
        public GameSpeed Speed { get; init; }
    }

    [MessageField(order: 0)]
    public int Number { get; init; }

    [MessageField(order: 1)]
    public required RoomParameter Parameter { get; init; }
}