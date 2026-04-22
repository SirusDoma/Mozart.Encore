using Encore.Messaging;

namespace Memoryer.Messages.Events;

public class DownloadProgressChangedEventData : IMessage
{
    public static Enum Command => EventCommand.DownloadProgressChanged;

    [MessageField(order: 0)]
    public byte Percentage { get; init; }

    [MessageField(order: 1)]
    public byte MemberId { get; init; }
}
