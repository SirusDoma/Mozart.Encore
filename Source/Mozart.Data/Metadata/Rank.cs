namespace Mozart.Metadata;

public enum Rank : uint
{
    None    = 0,
    Perfect = 1,
    S       = 2,
    A       = 3,
    B       = 4,
    C       = 5,
    D       = 6,
    E       = 7,
    F       = 8
}

public enum RankExtended : uint
{
    None      = 0,
    Perfect   = 1,
    SPlusPlus = 2,
    SPlus     = 3,
    S         = 4,
    APlusPlus = 5,
    APlus     = 6,
    A         = 7,
    B         = 8,
    C         = 9,
    D         = 10,
    E         = 11,
    F         = 12
}

public static class RankExtensions
{
    public static RankExtended FromPercentage(float percentage, bool allCombo)
    {
        if (percentage < 0.4f)
            return RankExtended.F;

        if (percentage < 0.5f)
            return RankExtended.E;

        if (percentage < 0.6f)
            return RankExtended.D;

        if (percentage < 0.7f)
            return RankExtended.C;

        if (percentage < 0.8f)
            return RankExtended.B;

        if (percentage < 0.9f)
            return allCombo ? RankExtended.APlus : RankExtended.A;

        if (percentage < 0.95f)
            return allCombo ? RankExtended.SPlus : RankExtended.APlusPlus;

        if (percentage >= 0.95f)
            return allCombo ? RankExtended.SPlusPlus : RankExtended.SPlus;

        return percentage >= 1.0f ? RankExtended.Perfect : RankExtended.None;
    }

    public static Rank ToRank(this RankExtended ext)
    {
        return ext switch
        {
            RankExtended.None                                              => Rank.None,
            RankExtended.Perfect                                           => Rank.Perfect,
            RankExtended.SPlusPlus or RankExtended.SPlus or RankExtended.S => Rank.S,
            RankExtended.APlusPlus or RankExtended.APlus or RankExtended.A => Rank.A,
            RankExtended.B                                                 => Rank.B,
            RankExtended.C                                                 => Rank.C,
            RankExtended.D                                                 => Rank.D,
            RankExtended.E                                                 => Rank.E,
            RankExtended.F                                                 => Rank.F,
            _ => throw new ArgumentOutOfRangeException(nameof(ext), ext, null)
        };
    }
}
