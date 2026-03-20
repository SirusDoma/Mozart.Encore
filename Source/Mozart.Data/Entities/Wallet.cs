namespace Mozart.Data.Entities;

public class Wallet
{
    public int UserId { get; init; }

    public int Gem { get; set; }

    public int O2Cash { get; set; }

    public int MusicCash { get; set; }

    public int ItemCash { get; set; }

    public int CashPoint { get; set; }

    public int Point { get; set; }
}
