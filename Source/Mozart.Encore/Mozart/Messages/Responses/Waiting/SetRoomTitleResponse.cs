using Encore.Messaging;

namespace Mozart;

public class SetRoomTitleResponse : IMessage
{
    public static Enum Command => ResponseCommand.SetRoomTitle;

    [StringMessageField(maxLength: 21)]
    public required string Title { get; init; }
}