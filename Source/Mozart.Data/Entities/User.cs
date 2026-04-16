using System.ComponentModel.DataAnnotations.Schema;
using Mozart.Metadata;

namespace Mozart.Data.Entities;

public class User
{
    public int Id { get; init; }

    public required string Username { get; init; }

    public required string Nickname { get; init; }

    public required Gender Gender { get; init; }

    public int Level { get; set; } = 1;

    public int Battle { get; set; }

    public int Win { get; set; }

    public int Lose { get; set; }

    public int Draw { get; set; }

    public int Experience { get; set; }

    public required bool IsAdministrator { get; init; }

    public int Ranking => UserRankingExtended.Ranking;

    public RankDeltaType RankDeltaType => UserRankingExtended.ChangeType == 0
        ? RankDeltaType.Down
        : RankDeltaType.Up;

    public int RankDelta => UserRankingExtended.ChangeRanking;

    [NotMapped]
    public int Gem
    {
        get => Wallet.Gem;
        set => Wallet.Gem = value;
    }

    [NotMapped]
    public int Point
    {
        get => Wallet.O2Cash;
        set => Wallet.O2Cash = value;
    }

    public int Ranking => UserRanking.Ranking;

    private Wallet Wallet { get; init; } = new();

    private Loadout Loadout { get; init; } = new();

    private List<AttributiveItem> AttributiveItems { get; init; } = [];

    private UserRanking UserRanking { get; init; } = new();

    private List<GiftItem> GiftItems { get; init; } = [];

    private List<GiftMusic> GiftMusics { get; init; } = [];

    private List<UserMessage> UserMessages { get; init; } = [];

    public List<AcquiredMusic> AcquiredMusicList { get; init; } = [];

    [NotMapped]
    public Inventory Inventory => new(Loadout, AttributiveItems);

    [NotMapped]
    public GiftBox GiftBox => new(this, GiftItems, GiftMusics);

    [NotMapped]
    public IReadOnlyList<GiftMessage> GiftMessages =>
        UserMessages.Where(m => !m.IsRead).Select(m => new GiftMessage(m)).ToList();

    [NotMapped]
    public EquipmentItems Equipments => new(Loadout);

    private IReadOnlyList<UserMessage> UserMessages { get; init; } = [];

    public List<AcquiredMusic> AcquiredMusicList { get; init; } = [];

    public List<MusicScoreRecord> MusicScoreRecords { get; init; } = [];

    private Member Member { get; init; } = null!;

    private Wallet Wallet { get; init; } = new();

    private Penalty Penalty { get; init; } = new();

    private Loadout Loadout { get; init; } = new();

    private List<AttributiveItem> AttributiveItems { get; init; } = [];

    private UserRanking UserRanking { get; init; } = new();

    private UserRankingExtended UserRankingExtended { get; init; } = new();

    private List<GiftItem> GiftItems { get; init; } = [];

    private List<GiftMusic> GiftMusics { get; init; } = [];
}
