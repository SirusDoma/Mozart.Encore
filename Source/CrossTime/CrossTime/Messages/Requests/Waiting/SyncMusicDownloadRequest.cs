using Encore.Messaging;

namespace CrossTime.Messages.Requests;

public class SyncMusicDownloadRequest : IMessage
{
    public static Enum Command => RequestCommand.SyncMusicDownload;

    [MessageField(order: 0)]
    public ushort MusicId { get; init; }
}
