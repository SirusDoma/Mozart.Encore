using Encore.Messaging;

namespace Amadeus.Messages.Events;

public class AlbumScoreCompletedEventData : IMessage
{
    public static Enum Command => EventCommand.AlbumScoreCompleted;

    [CollectionMessageField(order: 0, maxCount: 8, prefixSizeType: TypeCode.Int32)]
    public required IList<ScoreCompletedEventData.ScoreEntry> Scores { get; init; } = [];
}
