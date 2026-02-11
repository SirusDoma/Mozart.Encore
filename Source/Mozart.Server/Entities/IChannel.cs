using Mozart.Metadata.Items;
using Mozart.Metadata.Music;
using Mozart.Sessions;

namespace Mozart.Entities;


public interface IChannel : IBroadcastable
{
    int Id          { get; }
    int Capacity    { get; }
    int UserCount   { get; }
    float GemRates  { get; }
    float ExpRates  { get; }
    bool? FreeMusic { get; }

    string MusicListFileName { get; }
    string AlbumListFileName { get; }
    string ItemDataFileName  { get; }

    void Register(Session session);

    void Remove(Session session);

    IReadOnlyDictionary<int, MusicHeader> GetMusicList();
    IReadOnlyDictionary<int, AlbumHeader> GetAlbumList();
    IReadOnlyDictionary<int, ItemData> GetItemData();
}
