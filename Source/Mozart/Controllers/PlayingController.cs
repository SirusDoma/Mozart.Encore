using Microsoft.Extensions.Logging;

using Encore.Server;
using Microsoft.Extensions.Options;
using Mozart.Controllers.Filters;
using Mozart.Entities;
using Mozart.Messages;
using Mozart.Messages.Events;
using Mozart.Messages.Requests;
using Mozart.Options;
using Mozart.Persistence.Repositories;
using Mozart.Services;
using Mozart.Sessions;

namespace Mozart.Controllers;

[RoomAuthorize]
public class PlayingController(Session session, IUserRepository repository,
    IOptions<GameOptions> gameOptions, ILogger<WaitingController> logger) : CommandController<Session>(session)
{
    private IChannel Channel => Session.Channel!;

    private IRoom Room => Session.Room!;

    private IScoreTracker Tracker => Room.ScoreTracker;

    [CommandHandler]
    public async Task UpdateGameStats(UpdateGameStatsRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation((int)RequestCommand.UpdateGameStats,
            "Game [{RoomId:000}] [{User}] Update {Type}: {Value}",
            Room.Id, Session.Actor.Nickname, request.Type, request.Value);

        if (request.Type == GameUpdateStatsType.Life)
            await Tracker.UpdateLife(Session, request.Value, cancellationToken);
        else if (request.Type == GameUpdateStatsType.Jam)
            await Tracker.UpdateJamCombo(Session, request.Value, cancellationToken);
    }

    [CommandHandler]
    public async Task SubmitScore(ScoreSubmissionRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation((int)RequestCommand.SubmitScore,
            "Game [{RoomId:000}] [{User}] Submit score: {Value}", Room.Id, Session.Actor.Nickname, request.Score);

        var scores = await Tracker.SubmitScore(Session, request, cancellationToken);
        if (scores.Count < Room.UserCount)
            return;

         // TODO: Implement proper OJNList
        static int CountTotalNotes(ScoreTracker.UserScore score)
            => score.Cool + score.Good + score.Bad + score.Miss;

        if (scores.Where(s => s.Clear).GroupBy(CountTotalNotes).Count() > 1)
            throw new InvalidOperationException("Unbalance total notes"); // someone probably cheating?

        var entries    = new List<GameCompletedEventData.ScoreEntry>();
        bool safe      = scores.Any(s => s.Life > 0);
        int totalNotes = scores.Max(CountTotalNotes);

        var options = gameOptions.Value;
        for (int id = 0; id < Entities.Room.MaxCapacity; id++)
        {
            var state = scores.SingleOrDefault(e => e.MemberId == id);
            if (state == null)
                continue;

            // Compute reward only when it is safe
            // (because we have no information about total notes without OJNList)
            int reward = 0;
            if (safe && (Room.Metadata.Mode != GameMode.Single || options.SingleModeRewardLevelLimit == 0 || state.Session.Actor.Level < options.SingleModeRewardLevelLimit))
            {
                var user = await repository.Find(Session.Actor.UserId, cancellationToken);
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

                reward = (int)(reward * Channel.GemRates);

                int nextUserLevel = user.Level + 1;
                int xpNext = (int)(2.8333f * (2 * Math.Pow(nextUserLevel, 2.0f) + (3 * Math.Pow(nextUserLevel, 2.0f)
                    + (307 * nextUserLevel))));

                const int level = 15; // music level
                int xpGain = (int)(25 * (level + 3) * (state.Cool + (0.5 * state.Good)) / totalNotes);

                user.Gem        += reward;
                user.Experience += (int)(xpGain * Channel.ExpRates);
                if (user.Experience > xpNext)
                    user.Level++;

                await repository.Commit(cancellationToken);
                Session.Actor.Sync(user);
            }

            entries.Add(new GameCompletedEventData.ScoreEntry
            {
                MemberId   = (byte)id,
                Active     = true,
                Cool       = (ushort)state.Cool,
                Good       = (ushort)state.Good,
                Bad        = (ushort)state.Bad,
                Miss       = (ushort)state.Miss,
                MaxCombo   = (ushort)state.MaxCombo,
                JamCombo   = (ushort)state.MaxJamCombo,
                Score      = (ushort)state.Score,
                Reward     = (ushort)Math.Max(0, reward),
                Level      = Session.Actor.Level,
                Experience = Session.Actor.Experience,
                Win        = scores.Max(s => s.Score) == state.Score,
            });
        }

        await Room.Broadcast(new GameCompletedEventData
        {
            Scores = entries
        }, cancellationToken);

        await Channel.Broadcast(new RoomStateChangedEventData
        {
            Number = Room.Id,
            State = RoomState.Waiting
        }, cancellationToken);
    }

    [CommandHandler(RequestCommand.ExitPlaying)]
    public async Task ExitPlaying(CancellationToken cancellationToken)
    {
        logger.LogInformation((int)RequestCommand.ExitPlaying,
            "Leave game: [{RoomId:000}]", Room.Id);

        await Tracker.Untrack(Session, cancellationToken);
    }
}