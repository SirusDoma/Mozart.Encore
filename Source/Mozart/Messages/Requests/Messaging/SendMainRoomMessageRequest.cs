using Encore.Messaging;

namespace Mozart.Messages.Requests;

public class SendMainRoomMessageRequest : IMessage
{
    public static Enum Command => RequestCommand.SendMainRoomMessage;

    [StringMessageField(order: 0)]
    public required string Content { get; init; }
}