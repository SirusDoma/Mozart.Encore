using Encore.Messaging;

namespace Encore.Messages.Responses;

public class BugleMessageResponse : IMessage
{
    public static Enum Command => GameManagerCommand.BugleMessage;

    [StringMessageField(order: 0)]
    public string SenderNickname { get; init; } = string.Empty;

    [MessageField(order: 1)]
    public byte MessageType { get; init; }

    [StringMessageField(order: 2)]
    public string Content { get; init; } = string.Empty;
}
