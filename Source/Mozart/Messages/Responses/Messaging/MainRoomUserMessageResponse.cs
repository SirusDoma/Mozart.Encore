using Encore.Messaging;

namespace Mozart.Messages.Responses;

public class MainRoomUserMessageResponse : IMessage
{
    public static Enum Command => ResponseCommand.MainRoomUserMessage;

    [StringMessageField(order: 0)]
    public required string Sender { get; init; }

    [StringMessageField(order: 1)]
    public required string Content { get; init; }
}