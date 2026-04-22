using Encore.Messaging;

namespace Memoryer.Messages.Requests;

public class SyncMusicDownloadRequest : IMessage
{
    public static Enum Command => RequestCommand.SyncMusicDownload;

    [MessageField(order: 0)]
    public ushort MusicId { get; init; }
}
