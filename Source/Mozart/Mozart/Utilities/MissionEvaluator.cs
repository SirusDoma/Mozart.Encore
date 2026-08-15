using Encore.Metadata;
using Mozart.Metadata;
using Mozart.Metadata.Music;
using Mozart.Services;

using static Mozart.Messages.Events.ScoreCompletedEventData;

namespace Mozart;

public static class MissionEvaluator
{
    public static MissionResult Evaluate(MusicHeader? header, Difficulty difficulty, GameSpeed speed,
        ScoreTracker.UserScore score)
    {
        int playerLevel = score.Session.Actor.Level;
        if (playerLevel <= 0 || playerLevel >= 100 || (playerLevel + 1) % 4 != 0)
            return MissionResult.None;

        int mission = (playerLevel + 1) / 4;
        if (mission > 12)
            return MissionResult.None; // The client defines no mission beyond level 47

        if (header == null)
            return MissionResult.Failed;

        short level = difficulty switch
        {
            Difficulty.EX => header.LevelEx,
            Difficulty.NX => header.LevelNx,
            Difficulty.HX => header.LevelHx,
            _             => 0
        };

        int noteCount = difficulty switch
        {
            Difficulty.EX => header.NoteCountEx,
            Difficulty.NX => header.NoteCountNx,
            Difficulty.HX => header.NoteCountHx,
            _             => 0
        };

        if (noteCount == 0)
            return MissionResult.Failed;

        bool passed = mission switch
        {
            1  => level >= 4  && score is { Clear: true, MaxCombo: >= 50 },
            2  => level >= 5  && score.Clear && speed == GameSpeed.X05,
            3  => level >= 7  && score is { Clear: true, Life: >= 1000 },
            4  => level >= 9  && score.Clear && score.Cool * 100 / noteCount >= 70,
            5  => level >= 11 && score is { Clear: true, MaxJamCombo: >= 15 },
            6  => level >= 12 && score.Clear && score.Cool * 100 / noteCount >= 80,
            7  => level >= 14 && score.Clear && speed == GameSpeed.X60,
            8  => level >= 16 && score is { Clear: true, MaxJamCombo: >= 20 },
            9  => level >= 16 && score is { Clear: true, Life: >= 1000 } && speed == GameSpeed.X10,
            10 => level >= 17 && score is { Clear: true, MaxJamCombo: 0 },
            11 => level >= 17 && score is { Clear: true, Bad: 0, Miss: 0 },
            12 => level >= 18 && score.Clear && score.Cool * 100 / noteCount >= 80,
            _  => throw new ArgumentOutOfRangeException(nameof(score), mission, null)
        };

        return passed ? MissionResult.Completed : MissionResult.Failed;
    }
}
