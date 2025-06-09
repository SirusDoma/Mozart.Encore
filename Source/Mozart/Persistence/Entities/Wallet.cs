namespace Mozart.Persistence.Entities;

public class Wallet
{
    public int UserId { get; init; }

    public int Gem { get; set; }

    public int O2Cash { get; set; }
}