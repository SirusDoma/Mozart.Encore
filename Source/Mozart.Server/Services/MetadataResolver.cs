using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using Mozart.Entities;
using Mozart.Metadata.Items;
using Mozart.Metadata.Music;
using Mozart.Options;

namespace Mozart.Services;

public interface IMetadataResolver
{
    IReadOnlyDictionary<int, MusicHeader> GetMusicList();

    IReadOnlyDictionary<int, MusicHeader> GetMusicList(IChannel channel);

    IReadOnlyDictionary<int, AlbumHeader> GetAlbumList();

    IReadOnlyDictionary<int, AlbumHeader> GetAlbumList(IChannel channel);

    IReadOnlyDictionary<int, ItemData> GetItemData();

    IReadOnlyDictionary<int, ItemData> GetItemData(IChannel channel);
}

public class MetadataResolver(IOptions<MetadataOptions> defaultOptions) : IMetadataResolver
{
    private readonly ConcurrentDictionary<int, IReadOnlyDictionary<int, ItemData>> _itemCache = [];
    private readonly ConcurrentDictionary<int, IReadOnlyDictionary<int, MusicHeader>> _musicCache = [];
    private readonly ConcurrentDictionary<int, IReadOnlyDictionary<int, AlbumHeader>> _albumCache = [];

    public IReadOnlyDictionary<int, MusicHeader> GetMusicList()
    {
        return GetMusicList(-1, defaultOptions.Value.MusicList);
    }

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

    public IReadOnlyDictionary<int, AlbumHeader> GetAlbumList()
    {
        return GetAlbumList(-1, defaultOptions.Value.AlbumList);
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
            throw new FileNotFoundException("MusicList metadata file is not found", path);

        return _albumCache.GetOrAdd(id, static (_, p) =>
            AlbumListParser.Parse(File.OpenRead(p)), path);
    }

    public IReadOnlyDictionary<int, ItemData> GetItemData()
    {
        return GetItemData(-1, defaultOptions.Value.ItemData);
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
            ItemDataParser.Parse(File.ReadAllBytes(p)), path);
    }
}
