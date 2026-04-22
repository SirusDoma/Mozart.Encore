using Encore.Messaging;

namespace Memoryer.Messages.Requests;

public class AnnouncementRequest : IMessage
{
    public static Enum Command => RequestCommand.Announce;

    [StringMessageField(order: 0)]
    public required string Content { get; init; }
}
