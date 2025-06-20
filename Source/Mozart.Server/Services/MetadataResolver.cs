using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using Mozart.Entities;
using Mozart.Metadata.Items;
using Mozart.Options;

namespace Mozart.Services;

public interface IMetadataResolver
{
    IReadOnlyDictionary<int, ItemData> GetItemData(IChannel channel);
}

public class MetadataResolver(IOptions<MetadataOptions> defaultOptions) : IMetadataResolver
{
    private readonly ConcurrentDictionary<int, IReadOnlyDictionary<int, ItemData>> _itemCache = [];

    public IReadOnlyDictionary<int, ItemData> GetItemData(IChannel channel)
    {
        string path = channel.ItemDataFileName;
        if (string.IsNullOrEmpty(path))
            path = defaultOptions.Value.ItemData;

        return _itemCache.GetOrAdd(channel.Id, static (_, p) =>
            ItemDataParser.Parse(File.ReadAllBytes(p)), path);
    }
}