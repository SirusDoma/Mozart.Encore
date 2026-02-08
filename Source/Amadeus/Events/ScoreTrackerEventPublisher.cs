using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Amadeus.Messages;
using Amadeus.Messages.Events;

using Mozart.Entities;
using Mozart.Options;
using Mozart.Data.Repositories;
using Mozart.Events;
using Mozart.Metadata;
using Mozart.Services;

namespace Amadeus.Events;

public class ScoreTrackerEventPublisher(IUserRepository repository, IOptions<GameOptions> gameOptions,
    ILogger<ScoreTrackerEventPublisher> logger) : IEventPublisher<ScoreTracker>
{
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
                MemberId = (byte)e.MemberId,
                Type     = GameUpdateStatsType.Life,
                Value    = (ushort)e.Value
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
                MemberId = (byte)e.MemberId,
                Type     = GameUpdateStatsType.Jam,
                Value    = (ushort)e.Value
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
            if (scores.Where(s => s.Clear).Any(score => CountTotalNotes(score) > totalNotes))
                throw new InvalidOperationException("Unbalance total notes"); // someone probably cheating?

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

                // Compute reward only when it is safe
                int reward = 0;
                if (safe && (room.Metadata.Mode != GameMode.Single || options.SingleModeRewardLevelLimit == 0 || state.Session.Actor.Level < options.SingleModeRewardLevelLimit))
                {
                    var user = await repository.Find(state.Session.Actor.UserId, CancellationToken.None);
                    if (user == null)
                        throw new InvalidOperationException("User not found");

                    int maxJams    = (int)Math.Floor((totalNotes - 2f) / 25f);
                    int maxScore   = (200 * totalNotes) + (10 * maxJams * totalNotes) - (135 * maxJams) -
                                     (125 * maxJams * maxJams);

                    reward = (int)((((user.Level - 1f) / 5f) * 38f + 87f) * Math.Sqrt((float)state.Score / maxScore));
                    if (state is { Clear: true })
                        reward += 25; // Clear bonus

                    if (state is { Bad: 0, Miss: 0 })
                        reward += 25; // All combo bonus

                    if (state is { Good: 0, Bad: 0, Miss: 0 } or{ Cool: 0, Bad: 0, Miss: 0 } )
                        reward += 25; // All cool / good combo

                    reward = (int)(reward * channel.GemRates);

                    int nextUserLevel = user.Level + 1;
                    int xpNext = (int)(2.8333f * (2 * Math.Pow(nextUserLevel, 2.0f) + (3 * Math.Pow(nextUserLevel, 2.0f)
                        + (307 * nextUserLevel))));

                    int xpGain = (int)(25 * (level + 3) * (state.Cool + (0.5 * state.Good)) / totalNotes);

                    user.Gem        += reward;
                    user.Experience += (int)(xpGain * channel.ExpRates);
                    if (user.Experience > xpNext)
                        user.Level++;

                    await repository.Commit(CancellationToken.None);
                    state.Session.Actor.Sync(user);
                }

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
                    Win        = scores.Max(s => s.Score) == state.Score,
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