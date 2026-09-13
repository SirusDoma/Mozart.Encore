using Encore.Messaging;

namespace Encore.Messages.Requests;

public class AnnounceRequest : IMessage
{
    public static Enum Command => ServerCommand.Announce;

    [StringMessageField(order: 0)]
    public string Content { get; init; } = string.Empty;
}
