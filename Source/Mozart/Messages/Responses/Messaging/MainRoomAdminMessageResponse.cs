using Encore.Messaging;

namespace Mozart.Messages.Responses;

public class MainRoomAdminMessageResponse : IMessage
{
    public static Enum Command => ResponseCommand.MainRoomAdminMessage;

    [StringMessageField(order: 0)]
    public required string Sender { get; init; }

    [CollectionMessageField(order: 1)]
    public required byte[] Content { get; init; }
}