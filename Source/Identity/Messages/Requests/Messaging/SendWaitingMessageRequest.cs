using Encore.Messaging;

namespace Identity.Messages.Requests;

public class SendWaitingMessageRequest : IMessage
{
    public static Enum Command => RequestCommand.SendWaitingMessage;

    [CollectionMessageField(order: 0)]
    public required byte[] Content { get; init; }
}
