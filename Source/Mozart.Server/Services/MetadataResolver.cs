using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using Mozart.Entities;
using Mozart.Metadata.Items;
using Mozart.Metadata.Music;
using Mozart.Options;

namespace Mozart.Services;

public interface IMetadataResolver
{
    IReadOnlyList<MusicHeader> GetMusicList(IChannel channel);

    IReadOnlyDictionary<int, ItemData> GetItemData(IChannel channel);
}

public class MetadataResolver(IOptions<MetadataOptions> defaultOptions) : IMetadataResolver
{
    private readonly ConcurrentDictionary<int, IReadOnlyDictionary<int, ItemData>> _itemCache = [];
    private readonly ConcurrentDictionary<int, IReadOnlyList<MusicHeader>> _musicCache = [];

    public IReadOnlyList<MusicHeader> GetMusicList(IChannel channel)
    {
        string path = channel.MusicListFileName;
        if (string.IsNullOrEmpty(path))
            path = defaultOptions.Value.MusicList;

        if (!File.Exists(path))
            throw new FileNotFoundException("MusicList metadata file is not found", path);

        return _musicCache.GetOrAdd(channel.Id, static (_, p) =>
            MusicListParser.Parse(File.OpenRead(p)), path);
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