using Mozart.Data.Entities;
using Mozart.Metadata;
using Mozart.Metadata.Items;

namespace Mozart.Sessions;

public class Actor
{
    public Actor(User user)
    {
        UserId                = user.Id;
        Username              = user.Username;
        Nickname              = user.Nickname;
        Gender                = user.Gender;
        Gem                   = user.Gem;
        Point                 = user.Point;
        O2Cash                = user.O2Cash;
        MusicCash             = user.MusicCash;
        ItemCash              = user.ItemCash;
        CashPoint             = user.CashPoint;
        Level                 = user.Level;
        Battle                = user.Battle;
        Win                   = user.Win;
        Lose                  = user.Lose;
        Draw                  = user.Draw;
        Experience            = user.Experience;
        IsAdministrator       = user.IsAdministrator;
        Ranking               = user.Ranking;
        RankDeltaType         = user.RankDeltaType;
        RankDelta             = user.RankDelta;
        PenaltyLevel          = user.PenaltyLevel;
        PenaltyCount          = user.PenaltyCount;
        FreePass              = user.FreePass;
        StarterPass           = user.StarterPass;
        StarterPassExpiryDate = user.StarterPassExpiryDate;
        Equipments            = user.Equipments.ToDictionary(
            e => e.Key,
            e => (int)e.Value
        );
        Inventory        = user.Inventory.ToList();
        AcquiredMusicIds  = user.AcquiredMusicList.Select(m => (ushort)m.MusicId).ToList();
        MusicScoreRecords = user.MusicScoreRecords;
        GiftItems         = user.GiftBox.Items;
        GiftMusics        = user.GiftBox.Musics;
        GiftMessages      = user.GiftMessages;
    }

    public void Sync(User user)
    {
        Gem                   = user.Gem;
        Point                 = user.Point;
        Level                 = user.Level;
        Battle                = user.Battle;
        Win                   = user.Win;
        Lose                  = user.Lose;
        Draw                  = user.Draw;
        Experience            = user.Experience;
        Ranking               = user.Ranking;
        RankDeltaType         = user.RankDeltaType;
        RankDelta             = user.RankDelta;
        PenaltyLevel          = user.PenaltyLevel;
        PenaltyCount          = user.PenaltyCount;
        FreePass              = user.FreePass;
        StarterPass           = user.StarterPass;
        StarterPassExpiryDate = user.StarterPassExpiryDate;
        Equipments            = user.Equipments.ToDictionary(
            e => e.Key,
            e => (int)e.Value
        );
        Inventory         = user.Inventory.ToList();
        AcquiredMusicIds  = user.AcquiredMusicList.Select(m => (ushort)m.MusicId).ToList();
        MusicScoreRecords = user.MusicScoreRecords;
        GiftItems         = user.GiftBox.Items;
        GiftMusics        = user.GiftBox.Musics;
        GiftMessages      = user.GiftMessages;
    }

    public required string Token { get; init; }

    public int ServerId { get; set; }

    public required string ClientId { get; init; }

    public int UserId { get; init; }

    public string Username { get; init; }

    public string Nickname { get; init; }

    public Gender Gender { get; init; }

    public int Gem { get; set; }

    public int Point { get; set; }

    public int Ticket { get; set; }

    public int Level { get; set; }

    public int Battle { get; set; }

    public int Win { get; set; }

    public int Lose { get; set; }

    public int Draw { get; set; }

    public int Experience { get; set; }

    public int Ranking { get; set; }

    public RankDeltaType RankDeltaType { get; set; }

    public int RankDelta { get; set; }

    public int PenaltyLevel { get; set; }

    public int MembershipType { get; set; }

    public DateTime MembershipDate { get; set; }

    public bool IsAdministrator { get; init; }

    public Dictionary<ItemType, int> Equipments { get; set; }

    public IList<Inventory.BagItem> Inventory { get; set; }

    public IReadOnlyList<GiftItem> GiftItems { get; set; }

    public IReadOnlyList<GiftMusic> GiftMusics { get; set; }

    public IReadOnlyList<GiftMessage> GiftMessages { get; set; }

    public IReadOnlyList<ushort> AcquiredMusicIds { get; set; }

    public IReadOnlyList<MusicScoreRecord> MusicScoreRecords { get; set; }

    public IList<ushort> InstalledMusicIds { get; set; } = [];

    public IReadOnlyList<ushort> InstalledMusicIds { get; set; } = [];

    public override string ToString()
    {
        return Token;
    }
}
