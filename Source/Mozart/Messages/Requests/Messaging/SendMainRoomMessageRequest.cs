using Encore.Messaging;

namespace Mozart.Messages.Requests;

public class SendMainRoomMessageRequest : IMessage
{
    public static Enum Command => RequestCommand.SendMainRoomMessage;

    [CollectionMessageField(order: 0)]
    public required byte[] Content { get; init; }
}