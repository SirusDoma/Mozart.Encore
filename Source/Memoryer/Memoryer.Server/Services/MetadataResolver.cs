using System.Collections.Concurrent;
using Encore.Entities;
using Encore.Metadata;
using Encore.Metadata.Items;
using Encore.Metadata.Music;
using Encore.Options;
using Microsoft.Extensions.Options;
using Mozart.Metadata.Music;

namespace Mozart.Services;

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

        return GetMusicList(channel.Id, path);
    }

    private IReadOnlyDictionary<int, MusicHeader> GetMusicList(int id, string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("MusicList metadata file is not found", path);

        return _musicCache.GetOrAdd(id, static (_, p) =>
            MusicListParser.Parse(File.OpenRead(p)), path);
    }

    public IReadOnlyDictionary<int, AlbumHeader> GetAlbumList(IChannel channel)
    {
        string path = channel.AlbumListFileName;
        if (string.IsNullOrEmpty(path))
            path = defaultOptions.Value.AlbumList;

        return GetAlbumList(channel.Id, path);
    }

    private IReadOnlyDictionary<int, AlbumHeader> GetAlbumList(int id, string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("AlbumList metadata file is not found", path);

        return _albumCache.GetOrAdd(id, static (_, p) =>
            AlbumListParser.Parse(File.OpenRead(p)), path);
    }

    public IReadOnlyDictionary<int, ItemData> GetItemData(IChannel channel)
    {
        string path = channel.ItemDataFileName;
        if (string.IsNullOrEmpty(path))
            path = defaultOptions.Value.ItemData;

        return GetItemData(channel.Id, path);
    }

    private IReadOnlyDictionary<int, ItemData> GetItemData(int id, string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("ItemData metadata file is not found", path);

        return _itemCache.GetOrAdd(id, static (_, p) =>
            ItemDataParser.Parse(File.ReadAllBytes(p), ItemDataFormat.Classic), path);
    }
}
