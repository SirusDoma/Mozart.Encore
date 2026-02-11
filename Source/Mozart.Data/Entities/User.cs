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

    public List<AcquiredMusic> AcquiredMusicList { get; init; } = [];

    [NotMapped]
    public Inventory Inventory => new(Loadout, AttributiveItems);

    [NotMapped]
    public GiftBox GiftBox => new(this, GiftItems, GiftMusics);

    [NotMapped]
    public EquipmentItems Equipments => new(Loadout);
}