using Encore.Messaging;

namespace Memoryer.Messages.Requests;

public class SetDownloadProgressRequest : IMessage
{
    public static Enum Command => RequestCommand.SetDownloadProgress;

    [MessageField(order: 0)]
    public byte Progress { get; init; }

    [MessageField(order: 1)]
    public byte MemberId { get; init; }
}
