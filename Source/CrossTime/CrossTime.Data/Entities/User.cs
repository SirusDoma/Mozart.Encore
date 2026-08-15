using System.ComponentModel.DataAnnotations.Schema;
using Encore.Data.Entities;
using Encore.Metadata;

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
        get => Wallet.Point;
        set => Wallet.Point = value;
    }

    [NotMapped]
    public int O2Cash
    {
        get => Wallet.O2Cash;
        set => Wallet.O2Cash = value;
    }

    [NotMapped]
    public int MusicCash
    {
        get => Wallet.MusicCash;
        set => Wallet.MusicCash = value;
    }

    [NotMapped]
    public int ItemCash
    {
        get => Wallet.ItemCash;
        set => Wallet.ItemCash = value;
    }

    public int Ranking => UserRanking.Ranking;

    [NotMapped]
    public FreePass FreePass
    {
        get
        {
            var type = (FreePassType)Member.Vip;
            return type != FreePassType.None && Member.VipDate > DateTime.UtcNow
                ? new FreePass(type, Member.VipDate)
                : new FreePass(FreePassType.None, DateTime.UtcNow);
        }
        set
        {
            Member.Vip = (short)(uint)value.Type;
            Member.VipDate = value.ExpiryDate;
        }
    }

    private Member Member { get; init; } = new() { Username = string.Empty, Password = [] };

    private Wallet Wallet { get; init; } = new();

    private Loadout Loadout { get; init; } = new();

    private List<AttributiveItem> AttributiveItems { get; init; } = [];

    private UserRanking UserRanking { get; init; } = new();

    private List<GiftItem> GiftItems { get; init; } = [];

    private List<GiftMusic> GiftMusics { get; init; } = [];

    private List<UserMessage> UserMessages { get; init; } = [];

    public List<AcquiredMusic> AcquiredMusicList { get; init; } = [];

    public List<CompletedMission> CompletedMissionList { get; init; } = [];

    [NotMapped]
    public Inventory Inventory => new(Loadout, AttributiveItems);

    [NotMapped]
    public GiftBox GiftBox => new(this, GiftItems, GiftMusics);

    [NotMapped]
    public IReadOnlyList<GiftMessage> GiftMessages =>
        UserMessages.Where(m => !m.IsRead).Select(m => new GiftMessage(m)).ToList();

    [NotMapped]
    public EquipmentItems Equipments => new(Loadout);
}
