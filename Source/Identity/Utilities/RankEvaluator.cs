using Mozart.Metadata;
using Mozart.Metadata.Music;
using Mozart.Services;

namespace Identity;

public static class RankEvaluator
{
    public static Rank Evaluate(
        ScoreTracker.UserScore state,
        Difficulty difficulty,
        MusicHeader? header
    )
    {
        if (header == null)
            return Rank.None;

        int noteCount = difficulty switch
        {
            Difficulty.EX => header.NoteCountEx,
            Difficulty.NX => header.NoteCountNx,
            Difficulty.HX => header.NoteCountHx,
            _             => 0
        };

        if (noteCount == 0)
            return Rank.None;

        int maxJams   = (noteCount - 26) / 25;
        int remainder = (noteCount - 26) % 25;
        int maxScore  = 200 * noteCount
                        + (25 * 10) * maxJams * (maxJams + 1)
                        + (remainder > 0 ? 10 * remainder * (maxJams + 1) : 0);

        uint score = state.Score;

        if (!state.Clear)
            return Rank.F;

        if (score >= maxScore)
            return state.Cool == noteCount ? Rank.Perfect : Rank.S;

        bool allCombo = state.MaxCombo == noteCount - 1;
        if (score >= (int)(maxScore * 0.80) && allCombo)
            return Rank.S;
        if (score >= (int)(maxScore * 0.80))
            return Rank.A;
        if (score >= (int)(maxScore * 0.70))
            return Rank.B;
        if (score >= (int)(maxScore * 0.50))
            return Rank.C;

        return score >= (int)(maxScore * 0.30) ? Rank.D : Rank.E;
    }

    public static Rank Evaluate(
        uint score,
        Difficulty difficulty,
        MusicHeader? header,
        ClearType type
    )
    {
        if (header == null)
            return Rank.None;

        int noteCount = difficulty switch
        {
            Difficulty.EX => header.NoteCountEx,
            Difficulty.NX => header.NoteCountNx,
            Difficulty.HX => header.NoteCountHx,
            _             => 0
        };

        if (noteCount == 0)
            return Rank.None;

        int maxJams   = (noteCount - 26) / 25;
        int remainder = (noteCount - 26) % 25;
        int maxScore  = 200 * noteCount
                        + (25 * 10) * maxJams * (maxJams + 1)
                        + (remainder > 0 ? 10 * remainder * (maxJams + 1) : 0);

        if (score == 0)
            return Rank.F;

        if (type == ClearType.AllCool)
            return Rank.Perfect;

        if (score >= (int)(maxScore * 0.80) && type ==  ClearType.AllCombo)
            return Rank.S;
        if (score >= (int)(maxScore * 0.80))
            return Rank.A;
        if (score >= (int)(maxScore * 0.70))
            return Rank.B;
        if (score >= (int)(maxScore * 0.50))
            return Rank.C;

        return score >= (int)(maxScore * 0.30) ? Rank.D : Rank.E;
    }
}
