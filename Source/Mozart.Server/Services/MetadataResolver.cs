using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using Mozart.Entities;
using Mozart.Metadata.Items;
using Mozart.Metadata.Music;
using Mozart.Options;

namespace Mozart.Services;

public interface IMetadataResolver
{
    IReadOnlyDictionary<int, MusicHeader> GetMusicList(IChannel channel);

    IReadOnlyDictionary<int, AlbumHeader> GetAlbumList(IChannel channel);

    IReadOnlyDictionary<int, ItemData> GetItemData(IChannel channel);
}

public class MetadataResolver(IOptions<MetadataOptions> defaultOptions) : IMetadataResolver
{
    private readonly ConcurrentDictionary<int, IReadOnlyDictionary<int, ItemData>> _itemCache = [];
    private readonly ConcurrentDictionary<int, IReadOnlyDictionary<int, MusicHeader>> _musicCache = [];
    private readonly ConcurrentDictionary<int, IReadOnlyDictionary<int, AlbumHeader>> _albumCache = [];

    public IReadOnlyDictionary<int, MusicHeader> GetMusicList(IChannel channel)
    {
        string path = channel.MusicListFileName;
        if (string.IsNullOrEmpty(path))
            path = defaultOptions.Value.MusicList;

        if (!File.Exists(path))
            throw new FileNotFoundException("MusicList metadata file is not found", path);

        return _musicCache.GetOrAdd(channel.Id, static (_, p) =>
            MusicListParser.Parse(File.OpenRead(p)), path);
    }

    public IReadOnlyDictionary<int, AlbumHeader> GetAlbumList(IChannel channel)
    {
        string path = channel.AlbumListFileName;
        if (string.IsNullOrEmpty(path))
            path = defaultOptions.Value.AlbumList;

        if (!File.Exists(path))
            throw new FileNotFoundException("MusicList metadata file is not found", path);

        return _albumCache.GetOrAdd(channel.Id, static (_, p) =>
            AlbumListParser.Parse(File.OpenRead(p)), path);
    }

    public IReadOnlyDictionary<int, ItemData> GetItemData(IChannel channel)
    {
        string path = channel.ItemDataFileName;
        if (string.IsNullOrEmpty(path))
            path = defaultOptions.Value.ItemData;

        if (!File.Exists(path))
            throw new FileNotFoundException("ItemData metadata file is not found", path);

        return _itemCache.GetOrAdd(channel.Id, static (_, p) =>
            ItemDataParser.Parse(File.ReadAllBytes(p)), path);
    }
}
