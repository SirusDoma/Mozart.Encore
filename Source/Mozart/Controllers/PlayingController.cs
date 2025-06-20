using Microsoft.Extensions.Logging;

using Encore.Server;

using Mozart.Controllers.Filters;
using Mozart.Entities;
using Mozart.Messages;
using Mozart.Messages.Events;
using Mozart.Messages.Requests;
using Mozart.Metadata;
using Mozart.Services;
using Mozart.Sessions;

namespace Mozart.Controllers;

[RoomAuthorize]
public class PlayingController(Session session, ILogger<WaitingController> logger) : CommandController<Session>(session)
{
    private IRoom Room => Session.Room!;

    private IScoreTracker Tracker => Session.Room!.ScoreTracker;

    [CommandHandler(RequestCommand.ConfirmMusicLoaded)]
    public void ConfirmMusicLoaded()
    {
        logger.LogInformation((int)RequestCommand.ConfirmMusicLoaded,
            "User music loaded: [{RoomId:000}]", Room.Id);

        var slots = Room.Slots.ToList();
        int memberId = slots.FindIndex(s => s is Room.MemberSlot m && m.Session == Session);
        if (memberId < 0)
            return; // request forged?

        if (slots[memberId] is not Room.MemberSlot member || member.Session != Session)
            return; // request forged?

        Tracker.Track(Session);
    }

    [CommandHandler]
    public void UpdateGameStats(UpdateGameStatsRequest request)
    {
        logger.LogInformation((int)RequestCommand.UpdateGameStats,
            "Game [{RoomId:000}] [{User}] Update {Type}: {Value}",
            Room.Id, Session.Actor.Nickname, request.Type, request.Value);

        if (request.Type == GameUpdateStatsType.Life)
            Tracker.UpdateLife(Session, request.Value);
        else if (request.Type == GameUpdateStatsType.Jam)
            Tracker.UpdateJamCombo(Session, request.Value);
    }

    [CommandHandler]
    public void SubmitScore(ScoreSubmissionRequest request)
    {
        logger.LogInformation((int)RequestCommand.SubmitScore,
            "Game [{RoomId:000}] [{User}] Submit score: {Value}", Room.Id, Session.Actor.Nickname, request.Score);

        Tracker.SubmitScore(
            session:     Session,
            cool:        request.Cool,
            good:        request.Good,
            bad:         request.Bad,
            miss:        request.Miss,
            maxCombo:    request.MaxCombo,
            maxJamCombo: request.MaxJamCombo,
            score:       request.Score,
            life:        request.Life
        );
    }

    [CommandHandler(RequestCommand.ExitPlaying)]
    public void ExitPlaying()
    {
        logger.LogInformation((int)RequestCommand.ExitPlaying,
            "Leave game: [{RoomId:000}]", Room.Id);

        Tracker.Untrack(Session);
    }
}