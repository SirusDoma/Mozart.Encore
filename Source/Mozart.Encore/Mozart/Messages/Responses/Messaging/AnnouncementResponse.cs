using Encore.Messaging;

namespace Mozart;

public class AnnouncementResponse : IMessage
{
    public static Enum Command => ResponseCommand.Announcement;

    [StringMessageField(order: 0)]
    public required string Content { get; init; }
}