using Encore.Messaging;

namespace CrossTime.Messages.Requests;

public class SetRoomTitleRequest : IMessage
{
    public static Enum Command => RequestCommand.SetRoomTitle;

    [StringMessageField(maxLength: 21)]
    public required string Title { get; init; }
}
