using Encore.Messaging;

namespace Encore.Messages.Requests;

public class BugleMessageRequest : IMessage
{
    public static Enum Command => ServerCommand.BugleMessage;

    [MessageField(order: 0)]
    public uint SessionId { get; init; }

    [MessageField(order: 1)]
    public int UserId { get; init; }

    [MessageField(order: 2)]
    public byte MessageType { get; init; }

    [StringMessageField(order: 3)]
    public string Content { get; init; } = string.Empty;
}
