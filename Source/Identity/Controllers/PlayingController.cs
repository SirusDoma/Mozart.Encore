using Encore.Server;
using Memoryer.Controllers.Filters;
using Memoryer.Messages.Events;
using Memoryer.Messages.Requests;
using Memoryer.Messages.Responses;
using Memoryer.Relay.Messages.Requests;
using Memoryer.Services;
using Microsoft.Extensions.Logging;
using Mozart.Entities;
using Mozart.Metadata;
using Mozart.Services;
using Mozart.Sessions;

namespace Memoryer.Controllers;

[RoomAuthorize]
public class PlayingController(
    Session session,
    IRelayService relayService,
    ILogger<WaitingController> logger
) : CommandController<Session>(session)
{
    private IRoom Room => Session.Room!;

    private IScoreTracker Tracker => Session.Room!.ScoreTracker;

    [CommandHandler]
    public void ConfirmMusicLoaded(ConfirmMusicLoadedRequest request)
    {
        logger.LogInformation((int)RequestCommand.ConfirmMusicLoaded,
            "User music loaded: [{RoomId:000}] - {PowerSkill} / {Speed}", Room.Id, request.PowerSkillId, request.Speed);

        var slots = Room.Slots.ToList();
        int memberId = slots.FindIndex(s => s is Room.MemberSlot m && m.Session == Session);
        if (memberId < 0)
            return; // request forged?

        if (slots[memberId] is not Room.MemberSlot member || member.Session != Session)
            return; // request forged?

        Tracker.Track(Session, request.Speed);
        member.IsReady = false;
    }

    [CommandHandler(RequestCommand.ConfirmMusicLoadedEx)]
    public MusicLoadedExResponse ConfirmMusicLoadedEx()
    {
        logger.LogInformation((int)RequestCommand.ConfirmMusicLoaded,
            "User music loaded ex: [{RoomId:000}]", Room.Id);

        return new MusicLoadedExResponse();
    }

    [CommandHandler(RequestCommand.ReportUdpPunchHole, ResponseCommand.ReportUdpPunchHole)]
    public void ReportPunchHole()
    {
        logger.LogInformation(
            (int)RequestCommand.GetLiveState,
            "Report UDP hole punching [{RoomId:000}]",
            Room.Id
        );
    }

    [CommandHandler]
    public async Task TestNetworkLatency(TestNetworkLatencyRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation((int)RequestCommand.TestNetworkLatency,
            "Test network latency: [{MemberId} / {RT} / {LT} / {Seq} / {Unk}]",
            request.MemberId, request.RemoteTick, request.LocalTick, request.Sequence, request.Last);

        await Room.Broadcast(new TestNetworkLatencyCompletedEventData
        {
            MemberId   = request.MemberId,
            RemoteTick = Environment.TickCount,
            LocalTick  = request.LocalTick,
            Sequence   = request.Sequence,
            Last       = request.Last
        }, cancellationToken);

        if (request.Last)
        {
            Tracker.SyncTick(Session);
        }
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
