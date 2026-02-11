using Mozart.Metadata;
using Mozart.Metadata.Items;
using Mozart.Data.Entities;

namespace Mozart.Sessions;

public class Actor
{
    public Actor(User user)
    {
        UserId          = user.Id;
        Username        = user.Username;
        Nickname        = user.Nickname;
        Gender          = user.Gender;
        Gem             = user.Gem;
        Point           = user.Point;
        Level           = user.Level;
        Win             = user.Win;
        Lose            = user.Lose;
        Draw            = user.Draw;
        Experience      = user.Experience;
        Ranking         = user.Ranking;
        IsAdministrator = user.IsAdministrator;
        Equipments      = user.Equipments.ToDictionary(
            e => e.Key,
            e => (int)e.Value
        );
        Inventory        = user.Inventory.ToList();
        AcquiredMusicIds = user.AcquiredMusicList.Select(m => (ushort)m.MusicId).ToList();
        GiftItems        = user.GiftBox.Items;
        GiftMusics       = user.GiftBox.Musics;
    }

    public void Sync(User user)
    {
        Gem             = user.Gem;
        Point           = user.Point;
        Level           = user.Level;
        Win             = user.Win;
        Lose            = user.Lose;
        Draw            = user.Draw;
        Experience      = user.Experience;
        Ranking         = user.Ranking;
        Equipments      = user.Equipments.ToDictionary(
            e => e.Key,
            e => (int)e.Value
        );
        Inventory        = user.Inventory.ToList();
        AcquiredMusicIds = user.AcquiredMusicList.Select(m => (ushort)m.MusicId).ToList();
        GiftItems        = user.GiftBox.Items;
        GiftMusics       = user.GiftBox.Musics;
    }

    public required string Token { get; init; }

    public required string ClientId { get; init; }

    public int UserId { get; init; }

    public string Username { get; init; }

    public string Nickname { get; init; }

    public Gender Gender { get; init; }

    public int Gem { get; set; }

    public int Point { get; set; }

    public int Level { get; set; }

    public int Win { get; set; }

    public int Lose { get; set; }

    public int Draw { get; set; }

    public int Experience { get; set; }

    public int Ranking { get; set; }

    public bool IsAdministrator { get; init; }

    public Dictionary<ItemType, int> Equipments { get; set; }

    public IList<Inventory.BagItem> Inventory { get; set; }

    public IReadOnlyList<GiftItem> GiftItems { get; set; }

    public IReadOnlyList<GiftMusic> GiftMusics { get; set; }

    public IReadOnlyList<ushort> AcquiredMusicIds { get; set; }

    public IReadOnlyList<ushort> InstalledMusicIds { get; set; } = [];

    public override string ToString()
    {
        return Token;
    }
}