using Identity.Messages.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mozart.Data.Entities;
using Mozart.Data.Repositories;
using Mozart.Entities;
using Mozart.Events;
using Mozart.Metadata;
using Mozart.Options;
using Mozart.Services;

namespace Identity.Events;

public class ScoreTrackerEventPublisher(IUserRepository repository, IOptions<GameOptions> gameOptions,
    ILogger<ScoreTrackerEventPublisher> logger) : IEventPublisher<ScoreTracker>
{
    public readonly int[] NextLevelXp =
    [
        884, 1819, 2839, 3978, 5270, 6749, 8449, 10404, 12648, 15215,
        18139, 21454, 25194, 29393, 34085, 39304, 45084, 51459, 58463, 66130,
        74494, 83589, 93449, 104108, 115600, 127959, 141219, 155414, 170578, 186745,
        203949, 222224, 241604, 262123, 283815, 306714, 330854, 356269, 382993, 411060,
        440504, 471359, 503659, 537438, 572730, 609569, 647989, 688024, 729708, 773075,
        818159, 864994, 913614, 964053, 1016345, 1070524, 1126624, 1184679, 1244723, 1306790,
        1370914, 1437129, 1505469, 1575968, 1648660, 1723579, 1800759, 1880234, 1962038, 2046205,
        2132769, 2221764, 2313224, 2407183, 2503675, 2602734, 2704394, 2808689, 2915653, 3025320,
        3137724, 3252899, 3370879, 3491698, 3615390, 3741989, 3871529, 4004044, 4139568, 4278135,
        4419779, 4564534, 4712434, 4863513, 5017805, 5175344, 5336164, 5500299, 5667783, 6025912
    ];

    public void Monitor(ScoreTracker tracker)
    {
        tracker.UserTracked        += OnUserTracked;
        tracker.UserUntracked      += OnUserUntracked;
        tracker.UserLifeUpdated    += OnUserLifeUpdated;
        tracker.UserJamIncreased   += OnUserJamIncreased;
        tracker.UserScoreSubmitted += OnUserScoreSubmitted;
        tracker.ScoreCompleted     += OnScoreCompleted;
    }

    private IReadOnlyList<byte> ComputeMemberRanks(IReadOnlyList<ScoreTracker.UserScore> states)
    {
        return states
            .OrderByDescending(s => s.Score)
            .Select(s => (byte)s.MemberId)
            .Concat(Enumerable.Repeat(byte.MaxValue, Room.MaxCapacity))
            .Take(Room.MaxCapacity)
            .ToList();
    }

    private async void OnUserTracked(object? sender, ScoreTrackEventArgs e)
    {
        try
        {
            var tracker = (ScoreTracker)sender!;
            await tracker.Room.Broadcast(new MusicLoadedEventData
            {
                MemberId  = (byte)e.MemberId,
                IsPlaying = true
            }, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Failed to broadcast [ScoreTracker::OnUserTracked] event to one or more subscribers");
        }
    }

    private async void OnUserUntracked(object? sender, ScoreTrackEventArgs e)
    {
        try
        {
            var tracker = (ScoreTracker)sender!;
            var user = await repository.Find(e.Session.Actor.UserId);
            if (user != null)
                e.Session.Actor.Sync(user);

            await tracker.Room.Broadcast(new UserLeaveGameEventData
            {
                MemberId  = (byte)e.MemberId,
                Level     = e.Session.Actor.Level,
                CashPoint = e.Session.Actor.CashPoint,
                FreePass  = e.Session.Actor.FreePass
            }, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Failed to broadcast [ScoreTracker::OnUserUntracked] event to one or more subscribers");
        }
    }

    private async void OnUserLifeUpdated(object? sender, ScoreUpdateEventArgs e)
    {
        try
        {
            var tracker = (ScoreTracker)sender!;
            if (tracker.Completed ||tracker.Room.State != RoomState.Playing)
                return;

            await tracker.Room.Broadcast(new GameStatsUpdateEventData
            {
                MemberId    = (byte)e.MemberId,
                Type        = GameUpdateStatsType.Life,
                Value       = (ushort)e.Value,
                MemberRanks = ComputeMemberRanks(e.States)
            }, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Failed to broadcast [ScoreTracker::OnUserLifeUpdated] event to one or more subscribers");
        }
    }

    private async void OnUserJamIncreased(object? sender, ScoreUpdateEventArgs e)
    {
        try
        {
            var tracker = (ScoreTracker)sender!;
            if (tracker.Completed || tracker.Room.State != RoomState.Playing)
                return;

            await tracker.Room.Broadcast(new GameStatsUpdateEventData
            {
                MemberId    = (byte)e.MemberId,
                Type        = GameUpdateStatsType.Jam,
                Value       = (ushort)e.Value,
                MemberRanks = ComputeMemberRanks(e.States)
            }, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Failed to broadcast [ScoreTracker::OnUserJamIncreased] event to one or more subscribers");
        }
    }

    private async void OnUserScoreSubmitted(object? sender, ScoreSubmitEventArgs e)
    {
        try
        {
            var tracker = (ScoreTracker)sender!;
            await tracker.Room.Broadcast(new ScoreSubmissionEventData
            {
                MemberId = (byte)e.MemberId
            }, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Failed to broadcast [ScoreTracker::OnUserScoreSubmitted] event to one or more subscribers");
        }
    }

    private async void OnScoreCompleted(object? sender, ScoreTrackedEventArgs e)
    {
        try
        {
            e.Room.Channel.GetMusicList().TryGetValue(e.MusicId, out var music);

            var skills = e.Room.Channel.GetItemData()
                .Where(s => e.Room.Skills.Contains(s.Key) &&
                            s.Value.GameModifier != GameModifier.None)
                .Select(s => s.Value)
                .ToList();

            int level = e.Difficulty switch
            {
                Difficulty.EX => music?.LevelEx,
                Difficulty.NX => music?.LevelNx,
                _             => music?.LevelHx,
            } ?? 0;

            int totalNotes = e.Difficulty switch
            {
                Difficulty.EX => music?.NoteCountEx,
                Difficulty.NX => music?.NoteCountNx,
                _             => music?.NoteCountHx,
            } ?? 0;

            static int CountTotalNotes(ScoreTracker.UserScore score)
                => score.Cool + score.Good + score.Bad + score.Miss;

            var scores = e.States;
            if (e.Mode != GameMode.Jam)
            {
                if (scores.Where(s => s.Clear).Any(score => CountTotalNotes(score) > totalNotes))
                    throw new InvalidOperationException("Unbalance total notes"); // someone probably cheating?
            }

            var entries = new List<ScoreCompletedEventData.ScoreEntry>();
            bool safe   = music != null && scores.Any(s => s.Life > 0);

            var room    = e.Room;
            var channel = e.Room.Channel;
            var options = gameOptions.Value;

            for (int id = 0; id < Room.MaxCapacity; id++)
            {
                var state = scores.SingleOrDefault(m => m.MemberId == id);
                if (state == null)
                    continue;

                var mission = ScoreCompletedEventData.MissionResult.None;
                bool win  = scores.Max(s => s.Score) == state.Score;
                bool draw = scores.Count(s => s.Score == state.Score) > 1;

                var user = await repository.Find(state.Session.Actor.UserId);
                if (user == null)
                    throw new InvalidOperationException("User not found");

                // Compute reward only when it is safe
                int reward = 0;
                if (safe && (room.Metadata.Mode != GameMode.Single || options.SingleModeRewardLevelLimit == 0 || state.Session.Actor.Level < options.SingleModeRewardLevelLimit))
                {
                    int maxJams   = (totalNotes - 26) / 25;
                    int remainder = (totalNotes - 26) % 25;
                    int maxScore  = 200 * totalNotes
                                    + 25 * 10 * maxJams * (maxJams + 1)
                                    + (remainder > 0 ? 10 * remainder * (maxJams + 1) : 0);

                    reward = (int)((((user.Level - 1f) / 5f) * 38f + 87f) * Math.Sqrt((float)state.Score / maxScore));
                    if (state is { Clear: true })
                        reward += 25; // Clear bonus

                    if (state is { Bad: 0, Miss: 0 })
                        reward += 25; // All combo bonus

                    if (state is { Good: 0, Bad: 0, Miss: 0 } or{ Cool: 0, Bad: 0, Miss: 0 } )
                        reward += 25; // All cool / good combo

                    reward = (int)(reward * channel.GemRates);

                    int xpNext = user.Level >= 0 && user.Level < NextLevelXp.Length ? NextLevelXp[user.Level] : 0;
                    int xpGain = (int)(25 * (level + 3) * (state.Cool + (0.5 * state.Good)) / totalNotes);

                    user.Gem        += reward;
                    user.Experience += (int)(xpGain * channel.ExpRates);

                    if (user.Experience > xpNext)
                        mission = MissionEvaluator.Evaluate(music, e.Difficulty, skills, state);

                    if (e.Mode == GameMode.Jam && mission == ScoreCompletedEventData.MissionResult.Completed)
                        mission = ScoreCompletedEventData.MissionResult.Failed;

                    if (xpNext != 0 && user.Experience > xpNext)
                        user.Level++;
                }

                user.Battle++;
                if (draw)
                    user.Draw++;
                else if (win)
                    user.Win++;
                else
                    user.Lose++;

                var record = user.MusicScoreRecords.FirstOrDefault(r =>
                    r.MusicId == room.MusicId && r.Difficulty == room.Difficulty);

                bool newRecord = record == null || record.Score < state.Score;
                if (record == null)
                {
                    record = new MusicScoreRecord
                    {
                        UserId     = user.Id,
                        MusicId    = room.MusicId,
                        Difficulty = room.Difficulty
                    };

                    user.MusicScoreRecords.Add(record);
                }

                if (newRecord)
                {
                    record.Score = safe ? state.Score : 0;
                    record.ClearType = ClearType.None;

                    if (state.Cool == totalNotes)
                        record.ClearType = ClearType.AllCool;
                    else if (state.Cool + state.Good == totalNotes && state is { Bad: 0, Miss: 0 })
                        record.ClearType = ClearType.AllCombo;
                }

                await repository.Commit();
                state.Session.Actor.Sync(user);

                entries.Add(new ScoreCompletedEventData.ScoreEntry
                {
                    MemberId   = (byte)id,
                    Active     = true,
                    Cool       = (ushort)state.Cool,
                    Good       = (ushort)state.Good,
                    Bad        = (ushort)state.Bad,
                    Miss       = (ushort)state.Miss,
                    MaxCombo   = (ushort)state.MaxCombo,
                    JamCombo   = (ushort)state.MaxJamCombo,
                    Score      = state.Score,
                    Reward     = (ushort)Math.Max(0, reward),
                    Level      = state.Session.Actor.Level,
                    Experience = state.Session.Actor.Experience,
                    Result     = draw ? ScoreCompletedEventData.MatchResult.Draw :
                                 win  ? ScoreCompletedEventData.MatchResult.Win  :
                                        ScoreCompletedEventData.MatchResult.Lose,
                    Gem        = state.Session.Actor.Gem,
                    CashPoint  = state.Session.Actor.CashPoint,
                    Speed      = state.Speed,
                    Penalty    = state.LongNoteScore
                });
            }

            entries = entries.OrderByDescending(s => s.Score).ToList();
            switch (e.Mode)
            {
                case GameMode.Jam: await room.Broadcast(new AlbumScoreCompletedEventData { Scores = entries }, CancellationToken.None); break;
                default:           await room.Broadcast(new ScoreCompletedEventData { Scores = entries }, CancellationToken.None);      break;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Failed to broadcast [ScoreTracker::OnGameCompleted] event to one or more subscribers");
        }
    }
}
