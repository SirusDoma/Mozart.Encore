using Encore.Entities;
using Encore.Metadata.Items;
using Encore.Metadata.Music;
using Mozart.Metadata.Music;

namespace Encore.Metadata;

public interface IMetadataResolver
{
    IReadOnlyDictionary<int, MusicHeader> GetMusicList()
        => throw new NotSupportedException();

    IReadOnlyDictionary<int, MusicHeader> GetMusicList(IChannel channel);

    IReadOnlyDictionary<int, AlbumHeader> GetAlbumList()
        => throw new NotSupportedException();

    IReadOnlyDictionary<int, AlbumHeader> GetAlbumList(IChannel channel);

    IReadOnlyDictionary<int, ItemData> GetItemData()
        => throw new NotSupportedException();

    IReadOnlyDictionary<int, ItemData> GetItemData(IChannel channel);
}
