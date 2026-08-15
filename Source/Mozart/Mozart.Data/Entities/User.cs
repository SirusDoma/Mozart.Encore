using System.ComponentModel.DataAnnotations.Schema;
using Encore.Metadata;

namespace Mozart.Data.Entities;

public class User
{
    public int Id { get; init; }

    public required string Username { get; init; }

    public required string Nickname { get; set; }

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

    private Wallet Wallet { get; init; } = new();

    private Loadout Loadout { get; init; } = new();

    [NotMapped]
    public Inventory Inventory => new(Loadout);

    [NotMapped]
    public EquipmentItems Equipments => new(Loadout);
}
