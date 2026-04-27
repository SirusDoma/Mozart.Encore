using Identity.Controllers.Filters;
using Identity.Messages.Requests;
using Encore.Server;
using Microsoft.Extensions.Logging;
using Mozart.Entities;
using Mozart.Metadata;
using Mozart.Services;
using Mozart.Sessions;

namespace Identity.Controllers;

[RoomAuthorize]
public class PlayingController(Session session, ILogger<WaitingController> logger) : CommandController<Session>(session)
{
    private IRoom Room => Session.Room!;

    private IScoreTracker Tracker => Session.Room!.ScoreTracker;

    [CommandHandler]
    public void ConfirmMusicLoaded(ConfirmMusicLoadedRequest request)
    {
        logger.LogInformation((int)RequestCommand.ConfirmMusicLoaded,
            "User music loaded: [{RoomId:000}] - {PowerSkill}", Room.Id, request.PowerSkillId);

        var slots = Room.Slots.ToList();
        int memberId = slots.FindIndex(s => s is Room.MemberSlot m && m.Session == Session);
        if (memberId < 0)
            return; // request forged?

        if (slots[memberId] is not Room.MemberSlot member || member.Session != Session)
            return; // request forged?

        Tracker.Track(Session);
        member.IsReady = false;
    }

    [CommandHandler]
    public void UpdateGameStats(UpdateGameStatsRequest request)
    {
        logger.LogInformation((int)RequestCommand.UpdateGameStats,
            "Game [{RoomId:000}] Update #{Seq} - {Type}: {Value} (Bonus: {Bonus})",
            Room.Id, request.Sequence, request.Type, request.Value, request.LongNoteScore);

        if (request.Type == GameUpdateStatsType.Life)
            Tracker.UpdateLife(Session, request.Sequence, request.Value, request.Score, request.LongNoteScore);
        else if (request.Type == GameUpdateStatsType.Jam)
            Tracker.UpdateJamCombo(Session, request.Sequence, request.Value, request.Score, request.LongNoteScore);
    }

    [CommandHandler]
    public void SubmitScore(ScoreSubmissionRequest request)
    {
        logger.LogInformation((int)RequestCommand.SubmitScore,
            "Game [{RoomId:000}] Submit score: {Value}", Room.Id, request.Score);

        Tracker.SubmitScore(
            session: Session,
            cool: request.Cool,
            good: request.Good,
            bad: request.Bad,
            miss: request.Miss,
            maxCombo: request.MaxCombo,
            maxJamCombo: request.MaxJamCombo,
            score: request.Score,
            life: request.Life,
            speed: request.Speed,
            longNoteScore: request.LongNoteScore
        );
    }

    [CommandHandler(RequestCommand.FinalizeRank)]
    public void FinalizeRank()
    {
        logger.LogInformation((int)RequestCommand.FinalizeRank,
            "Finalize rank: [{RoomId:000}]", Room.Id);

        // await e.Room.Broadcast(new GameRankUpdateEventData
        // {
        //     MemberRanks = entries
        //         .Select(s => s.MemberId)
        //         .Concat(Enumerable.Repeat(byte.MaxValue, Room.MaxCapacity))
        //         .Take(Room.MaxCapacity)
        //         .ToList()
        // }, CancellationToken.None);
    }

    [CommandHandler(RequestCommand.ExitPlaying)]
    public void ExitPlaying()
    {
        logger.LogInformation((int)RequestCommand.ExitPlaying,
            "Leave game: [{RoomId:000}]", Room.Id);

        Tracker.Untrack(Session);
    }
}
