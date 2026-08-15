using Encore.Messaging;

namespace Mozart.Messages.Requests;

public class SyncMusicDownloadRequest : IMessage
{
    public static Enum Command => RequestCommand.SyncMusicDownload;

    [MessageField(order: 0)]
    public uint MusicId { get; init; }
}
