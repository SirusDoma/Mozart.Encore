using Encore.Messaging;

namespace Amadeus.Messages.Requests;

public class SendWaitingMessageRequest : IMessage
{
    public static Enum Command => RequestCommand.SendWaitingMessage;

    [CollectionMessageField(order: 0)]
    public required byte[] Content { get; init; }
}