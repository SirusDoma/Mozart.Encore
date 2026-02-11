using System.ComponentModel.DataAnnotations.Schema;
using Mozart.Metadata;

namespace Mozart.Data.Entities;

public class User
{
    public int Id { get; set; }

    public required string Username { get; set; }

    public required string Nickname { get; set; }

    public required Gender Gender { get; set; }

    public int Level { get; set; } = 1;

    public int Battle { get; set; }

    public int Win { get; set; }

    public int Lose { get; set; }

    public int Draw { get; set; }

    public int Experience { get; set; }

    public required bool IsAdministrator { get; set; }

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

    private Wallet Wallet { get; init; } = new();

    private Inventory Items { get; init; } = new();

    [NotMapped]
    public InventoryItems Inventory => new(Items);

    [NotMapped]
    public EquipmentItems Equipments => new(Items);
}
