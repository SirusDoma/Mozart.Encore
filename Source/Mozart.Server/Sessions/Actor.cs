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
        IsAdministrator = user.IsAdministrator;
        Equipments      = user.Equipments.ToDictionary(
            e => e.Key,
            e => (int)e.Value
        );
        Inventory = user.Inventory.Select(e => (int)e).ToList();
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
        Equipments      = user.Equipments.ToDictionary(
            e => e.Key,
            e => (int)e.Value
        );
        Inventory = user.Inventory.Select(e => (int)e).ToList();
    }

    public required string Token { get; init; }

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

    public bool IsAdministrator { get; init; }

    public Dictionary<ItemType, int> Equipments { get; set; }

    public IList<int> Inventory { get; set; }

    public IList<int> AttributiveItemIds { get; set; } = [];

    public IReadOnlyList<int> MusicIds { get; set; } = [];

    public override string ToString()
    {
        return Token;
    }
}