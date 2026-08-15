using CrossTime.Messages.Events;
using Encore.Data.Repositories;
using Encore.Events;
using Encore.Metadata;
using Microsoft.Extensions.Logging;
using Mozart.Entities;
using Mozart.Metadata;
using Mozart.Services;

namespace CrossTime.Events;

public class ScoreTrackerEventPublisher(
    IUserRepository repository,
    ILogger<ScoreTrackerEventPublisher> logger
) : IEventPublisher<ScoreTracker>
{
    private readonly SemaphoreSlim _mutex = new(1, 1);

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
        4419779, 4564534, 4712434, 4863513, 5017805, 5175344, 5336164, 5500299, 5667783
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

    private async void OnUserTracked(object? sender, ScoreTrackEventArgs e)
    {
        try
        {
            var tracker = (ScoreTracker)sender!;
            await tracker.Room.Broadcast(new MusicLoadedEventData
            {
                MemberId = (byte)e.MemberId
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
        await _mutex.WaitAsync();

        try
        {
            var tracker = (ScoreTracker)sender!;
            await tracker.Room.Broadcast(new UserLeaveGameEventData
            {
                MemberId = (byte)e.MemberId,
                Level    = e.Session.Actor.Level,
            }, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Failed to broadcast [ScoreTracker::OnUserUntracked] event to one or more subscribers");
        }
        finally
        {
            _mutex.Release();
        }
    }

    private IReadOnlyList<GameStatsUpdateEventData.MemberScore> ComputeMemberScores(
        IReadOnlyList<ScoreTracker.UserScore> states)
    {
        return states
            .OrderByDescending(s => s.Score)
            .Select(s => new GameStatsUpdateEventData.MemberScore
            {
                MemberId = (byte)s.MemberId,
                Score    = (int)s.Score
            })
            .Concat(Enumerable.Repeat(new GameStatsUpdateEventData.MemberScore
            {
                MemberId = byte.MaxValue,
                Score    = -1
            }, Room.MaxCapacity))
            .Take(Room.MaxCapacity)
            .ToList();
    }

    private async void OnUserLifeUpdated(object? sender, ScoreUpdateEventArgs e)
    {
        try
        {
            var tracker = (ScoreTracker)sender!;
            if (tracker.Room.State != RoomState.Playing)
                return;

            await tracker.Room.Broadcast(new GameStatsUpdateEventData
            {
                MemberId     = (byte)e.MemberId,
                Type         = GameUpdateStatsType.Life,
                Value        = (ushort)e.Value,
                MemberScores = ComputeMemberScores(e.States)
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
            if (tracker.Room.State != RoomState.Playing)
                return;

            await tracker.Room.Broadcast(new GameStatsUpdateEventData
            {
                MemberId     = (byte)e.MemberId,
                Type         = GameUpdateStatsType.Jam,
                Value        = (ushort)e.Value,
                MemberScores = ComputeMemberScores(e.States)
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
        await _mutex.WaitAsync();

        try
        {
            e.Room.Channel.GetMusicList().TryGetValue(e.MusicId, out var music);

            int level = e.Difficulty switch
            {
                Difficulty.EX => music?.LevelEx,
                Difficulty.NX => music?.LevelNx,
                _             => music?.LevelHx,
            } ?? 0;

            int noteCount = e.Difficulty switch
            {
                Difficulty.EX => music?.NoteCountEx,
                Difficulty.NX => music?.NoteCountNx,
                _             => music?.NoteCountHx,
            } ?? 0;

            static int CountTotalNotes(ScoreTracker.UserScore score)
                => score.Cool + score.Good + score.Bad + score.Miss;

            var scores  = e.States;
            bool safe   = music != null && scores.Any(s => s.Clear);

            if (safe && scores.Where(s => s.Clear).Any(score => CountTotalNotes(score) > noteCount))
                throw new InvalidOperationException("Unbalance total notes"); // someone probably cheating?

            var entries = new List<ScoreCompletedEventData.ScoreEntry>();

            var room    = e.Room;
            var channel = e.Room.Channel;

            for (int id = 0; id < Room.MaxCapacity; id++)
            {
                var state = scores.SingleOrDefault(m => m.MemberId == id);
                if (state == null)
                    continue;

                bool win  = scores.Max(s => s.Score) == state.Score;
                bool draw = scores.Count(s => s.Score == state.Score) > 1;
                int totalNotes = CountTotalNotes(state);
                float percentage = 0f;

                var user = await repository.Find(state.Session.Actor.UserId);
                if (user == null)
                    throw new InvalidOperationException("User not found");

                // Compute reward only when it is safe
                int rewardGem = 0;
                if (safe)
                {
                    int maxJams   = (totalNotes - 26) / 25;
                    int remainder = (totalNotes - 26) % 25;
                    int maxScore  = 200 * totalNotes
                                  + 25 * maxJams * (maxJams + 1)
                                  + (remainder > 0 ? 2 * remainder * (maxJams + 1) : 0);

                    percentage = Math.Clamp((float)state.Score / maxScore, 0f, 1f);

                    rewardGem = (int)((((user.Level - 1f) / 5f) * 38f + 87f) * Math.Sqrt((float)state.Score / maxScore));
                    if (state is { Clear: true })
                        rewardGem += 25; // Clear bonus

                    if (state is { Bad: 0, Miss: 0 })
                        rewardGem += 25; // All combo bonus

                    if (state is { Good: 0, Bad: 0, Miss: 0 } or{ Cool: 0, Bad: 0, Miss: 0 } )
                        rewardGem += 25; // All cool / good combo

                    rewardGem = Math.Max(0, (int)(rewardGem * channel.GemRates));

                    int xpNext = user.Level >= 0 && user.Level < NextLevelXp.Length ? NextLevelXp[user.Level] : 0;
                    int xpGain = (int)((25 * (level + 3) * (state.Cool + (0.5 * state.Good)) / totalNotes) * channel.ExpRates);

                    user.Gem       += rewardGem;
                    user.Experience = xpNext != 0
                        ? Math.Min(user.Experience + xpGain, xpNext)
                        : user.Experience + xpGain;

                    if (xpNext != 0 && user.Experience >= xpNext)
                        user.Level++;
                }

                user.Battle++;
                if (draw)
                    user.Draw++;
                else if (win)
                    user.Win++;
                else
                    user.Lose++;

                await repository.Commit();
                state.Session.Actor.Sync(user);

                entries.Add(new ScoreCompletedEventData.ScoreEntry
                {
                    MemberId      = (byte)id,
                    Active        = true,
                    Cool          = (ushort)state.Cool,
                    Good          = (ushort)state.Good,
                    Bad           = (ushort)state.Bad,
                    Miss          = (ushort)state.Miss,
                    MaxCombo      = (ushort)state.MaxCombo,
                    JamCombo      = (ushort)state.MaxJamCombo,
                    Score         = state.Score,
                    Gem           = state.Session.Actor.Gem - rewardGem,
                    Level         = state.Session.Actor.Level,
                    Experience    = state.Session.Actor.Experience,
                    Win           = win,
                    Draw          = draw,
                    RewardGemStar = 0,
                    RewardGem     = rewardGem,
                    GemStar       = state.Session.Actor.GemStar,
                    Rank          = RankExtensions.FromPercentage(percentage, state is { Bad: 0, Miss: 0 })
                });
            }

            entries = entries.OrderByDescending(s => s.Score).ToList();
            switch (e.Mode)
            {
                case GameMode.Jam:
                    var albumEntries = entries.Select(s => new AlbumScoreCompletedEventData.ScoreEntry
                    {
                        MemberId      = s.MemberId,
                        Active        = s.Active,
                        Cool          = s.Cool,
                        Good          = s.Good,
                        Bad           = s.Bad,
                        Miss          = s.Miss,
                        MaxCombo      = s.MaxCombo,
                        JamCombo      = s.JamCombo,
                        Score         = s.Score,
                        Gem           = s.Gem,
                        Level         = s.Level,
                        Experience    = s.Experience,
                        Win           = s.Win,
                        Draw          = s.Draw,
                        RewardGemStar = s.RewardGemStar,
                        RewardGem     = s.RewardGem,
                        GemStar       = s.GemStar,
                        Rank          = s.Rank
                    }).ToList();

                    await room.Broadcast(new AlbumScoreCompletedEventData { Scores = albumEntries }, CancellationToken.None);
                    break;
                default:
                    await room.Broadcast(new ScoreCompletedEventData { Scores = entries }, CancellationToken.None);
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Failed to broadcast [ScoreTracker::OnGameCompleted] event to one or more subscribers");
        }
        finally
        {
            _mutex.Release();
        }
    }
}
