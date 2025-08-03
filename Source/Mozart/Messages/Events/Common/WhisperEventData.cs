using Encore.Messaging;

namespace Mozart.Messages.Responses;

public class WhisperEventData : IMessage
{
    public static Enum Command => EventCommand.ReceiveWhisper;

    [StringMessageField(order: 0)]
    public required string Recipient { get; init; }

    [StringMessageField(order: 1)]
    public string Content { get; init; } = string.Empty;
}
