using Encore.Messaging;

namespace Mozart.Messages.Requests;

public class SendWaitingMessageRequest : IMessage
{
    public static Enum Command => RequestCommand.SendWaitingMessage;

    [StringMessageField(order: 0)]
    public required string Content { get; init; }
}