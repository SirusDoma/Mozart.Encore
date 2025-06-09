using Encore.Messaging;

namespace Mozart.Messages.Requests;

public class SendWhisperMessageRequest : IMessage
{
    public static Enum Command => RequestCommand.SendWhisper;

    [StringMessageField(order: 0)]
    public required string Recipient { get; init; }

    [StringMessageField(order: 1)]
    public required string Content { get; init; }
}