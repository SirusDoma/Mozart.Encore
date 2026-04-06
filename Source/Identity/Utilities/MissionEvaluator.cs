using Mozart.Metadata;
using Mozart.Metadata.Items;
using Mozart.Metadata.Music;
using Mozart.Services;

using static Identity.Messages.Events.ScoreCompletedEventData;

namespace Identity;

public static class MissionEvaluator
{
    public static MissionResult Evaluate(
        MusicHeader? header,
        Difficulty difficulty,
        IReadOnlyList<ItemData> items,
        ScoreTracker.UserScore score
    )
    {
        int playerLevel = score.Session.Actor.Level;
        if (playerLevel <= 0 || playerLevel >= 100 || (playerLevel + 1) % 4 != 0)
            return MissionResult.None;

        if (header == null)
            return MissionResult.Failed;

        int mission = (playerLevel + 1) / 4;
        if (mission == 0)
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

        bool HasModifier(GameModifier modifier) =>
            items.Any(i => i.GameModifier == modifier);

        bool passed = mission switch
        {
            1  => level >= 4  && score.Clear,
            2  => level >= 5  && score.MaxCombo >= 40,
            3  => level >= 7  && noteCount > 0 && score.Cool * 100 / noteCount >= 70,
            4  => level >= 9  && score.MaxCombo >= 100,
            5  => level >= 10 && score is { MaxJamCombo: >= 10, Clear: true },
            6  => level >= 11 && noteCount > 0 && score.Cool * 100 / noteCount >= 80,
            7  => level >= 12 && score.Miss <= 10,
            8  => level >= 14 && score is { MaxJamCombo: >= 20, Clear: true },
            9  => level >= 15 && HasModifier(GameModifier.Hidden) && score.Clear,
            10 => level >= 16 && score is { Speed: GameSpeed.X10, Clear: true },
            11 => level >= 17 && score is { Speed: GameSpeed.X60, Clear: true },
            12 => level >= 18 && noteCount > 0 && score.Cool * 100 / noteCount >= 90,
            13 => level >= 18 && noteCount > 0 && score.Good * 100 / noteCount >= 70,
            14 => level >= 17 && score is { Speed: GameSpeed.X05, Clear: true },
            15 => level >= 17 && HasModifier(GameModifier.Sudden) && noteCount > 0 && score.Cool * 100 / noteCount >= 80,
            16 => level >= 18 && score.Life >= 1000,
            17 => level >= 18 && HasModifier(GameModifier.Hidden) && noteCount > 0 && score.Cool * 100 / noteCount >= 80,
            18 => level >= 18 && score is { Bad: 0, Miss: 0 },
            19 => level >= 19 && score is { MaxJamCombo: 0, Clear: true },
            20 => level >= 19 && score is { Speed: GameSpeed.X10, Bad: 0, Miss: 0 },
            21 => level >= 18 && HasModifier(GameModifier.Dark) && score.Clear,
            22 => level >= 20 && noteCount > 0 && score.Cool * 100 / noteCount >= 95,
            23 => level >= 25 && score is { Bad: 0, Miss: 0 },
            24 => level >= 18 && noteCount > 0 && score.Good == noteCount,
            25 => level >= 31 && score is { Bad: 0, Miss: 0 },
            _ => throw new ArgumentOutOfRangeException(nameof(score), mission, null)
        };

        return passed ? MissionResult.Completed : MissionResult.Failed;
    }
}
