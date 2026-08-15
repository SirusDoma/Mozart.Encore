namespace Mozart.Data.Entities;

public class UserRankingExtended
{
    public int Id { get; init; }

    public int UserId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Nickname { get; set; } = string.Empty;

    public bool Sex { get; set; }

    public int Level { get; set; }

    public int Battle { get; set; }

    public int Win { get; set; }

    public int Draw { get; set; }

    public int Lose { get; set; }

    public int Experience { get; set; }

    public DateTime WriteDate { get; set; }

    public int Ranking { get; set; }

    public int ChangeType { get; set; }

    public int ChangeRanking { get; set; }
}
