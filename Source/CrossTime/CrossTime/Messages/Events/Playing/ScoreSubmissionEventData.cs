using Encore.Messaging;

namespace CrossTime.Messages.Events;

public class ScoreSubmissionEventData : IMessage
{
    public static Enum Command => EventCommand.ScoreSubmission;

    [MessageField(order: 0)]
    public byte MemberId { get; init; }
}
