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

    public int GemStar { get; set; }

    public int Ticket { get; set; } = 10;

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

    [NotMapped]
    public int MembershipType
    {
        get => Credential.MembershipType;
        set => Credential.MembershipType = value;
    }

    [NotMapped]
    public DateTime MembershipDate
    {
        get => Credential.MembershipDate;
        set => Credential.MembershipDate = value;
    }

    private Credential Credential { get; init; } = new() { Username = string.Empty, Password = [] };

    private Wallet Wallet { get; init; } = new();

    private Loadout Loadout { get; init; } = new();

    private List<AttributiveItem> AttributiveItems { get; init; } = [];

    private UserRanking UserRanking { get; init; } = new();

    private List<GiftItem> GiftItems { get; init; } = [];

    private List<GiftMusic> GiftMusics { get; init; } = [];

    public List<AcquiredMusic> AcquiredMusicList { get; init; } = [];

    public List<CompletedMission> CompletedMissionList { get; init; } = [];

    [NotMapped]
    public Inventory Inventory => new(Loadout, AttributiveItems);

    [NotMapped]
    public GiftBox GiftBox => new(this, GiftItems, GiftMusics);

    [NotMapped]
    public EquipmentItems Equipments => new(Loadout);
}
