using Encore.Messaging;
using Mozart.Metadata;

namespace Amadeus.Messages.Events;

public class WaitingAlbumChangedEventData : IMessage
{
    public static Enum Command => EventCommand.WaitingAlbumChanged;

    [MessageField(order: 0)]
    public int AlbumId { get; init; }

    [MessageField(order: 1)]
    public GameSpeed Speed { get; init; }
}