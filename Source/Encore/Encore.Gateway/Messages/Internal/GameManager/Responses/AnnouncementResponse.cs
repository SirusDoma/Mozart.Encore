using Encore.Messaging;

namespace Encore.Messages.Responses;

public class AnnouncementResponse : IMessage
{
    public static Enum Command => GameManagerCommand.Announcement;

    [StringMessageField(order: 0)]
    public string Content { get; init; } = string.Empty;
}
