using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Encore.Server;

using Mozart.Controllers.Filters;
using Mozart.Entities;
using Mozart.Messages;
using Mozart.Messages.Events;
using Mozart.Messages.Requests;
using Mozart.Messages.Responses;
using Mozart.Options;
using Mozart.Sessions;

namespace Mozart.Controllers;

[RoomAuthorize]
public class WaitingController(Session session, IOptions<GameOptions> options, ILogger<WaitingController> logger)
    : CommandController<Session>(session)
{
    private IChannel Channel => Session.Channel!;

    private IRoom Room => Session.Room!;

    [RoomMasterAuthorize]
    [CommandHandler]
    public async Task<WaitingMusicChangedEventData> SetRoomMusic(SetRoomMusicRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            (int)RequestCommand.SetRoomMusic,
            "Update room [{RoomId:000}] music settings: [{Difficulty} / {Speed} / o2ma{MusicId}]",
            Room.Id, request.Difficulty, request.Speed, request.MusicId
        );

        Room.MusicId = request.MusicId;
        Room.Difficulty = request.Difficulty;
        Room.Speed = request.Speed;
        await Room.SaveMetadataChanges(Session, cancellationToken);

        return new WaitingMusicChangedEventData
        {
            MusicId    = request.MusicId,
            Difficulty = request.Difficulty,
            Speed      = request.Speed
        };
    }

    [RoomMasterAuthorize]
    [CommandHandler]
    public async Task<WaitingRoomTitleEventData> SetRoomTitle(SetRoomTitleRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation((int)RequestCommand.SetRoomTitle,
            "Update room [{RoomId:000}] title: [{Title}]", Room.Id, request.Title);

        Room.Title = request.Title;
        await Room.SaveMetadataChanges(Session, cancellationToken);

        return new WaitingRoomTitleEventData
        {
            Title = request.Title[..Math.Min(21, request.Title.Length)]
        };
    }

    [RoomMasterAuthorize]
    [CommandHandler]
    public async Task<RoomArenaChangedEventData> SetRoomArena(SetRoomArenaRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            (int)RequestCommand.SetRoomArena,
            "Update room [{RoomId:000}] arena: [{Arena}] ({Seed})",
            Room.Id, request.Payload.Arena, request.Payload.RandomSeed
        );

        Room.Arena           = request.Payload.Arena;
        Room.ArenaRandomSeed = request.Payload.RandomSeed;
        await Room.SaveMetadataChanges(Session, cancellationToken);

        return new RoomArenaChangedEventData
        {
            Arena      = request.Payload.Arena,
            RandomSeed = request.Payload.RandomSeed
        };
    }

    [CommandHandler]
    public async Task SetRoomPlayerTeam(SetTeamRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation((int)RequestCommand.SetRoomTeam,
            "Update room [{RoomId:000}] [{User}] team: [{Team}]", Room.Id, Session.Actor.Nickname, request.Team);

        await Room.UpdateTeam(Session, request.Team, cancellationToken);
    }

    [CommandHandler]
    public Task SetPlayerInstrument(SetInstrumentRequest request, CancellationToken cancellationToken)
    {
        // Do not reply to this command:
        // The game was supposed to send "InstrumentId" but instead, it always sends us 0
        // return Task.FromResult(new PlayerInstrumentChangedEventData()
        // {
        //     MemberId     = 0,
        //     InstrumentId = request.InstrumentId
        // });

        return Task.CompletedTask;
    }

    [RoomMasterAuthorize]
    [CommandHandler]
    public async Task UpdateSlot(UpdateSlotRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation((int)RequestCommand.UpdateSlot,
            "Update room [{RoomId:000}] slot: [{MemberId}]", Room.Id, request.MemberId);

        await Room.UpdateSlot(Session, request.MemberId, cancellationToken);
    }

    [CommandHandler(RequestCommand.StartGame)]
    [RoomMasterAuthorize]
    public async Task<StartGameEventData> StartGame(CancellationToken cancellationToken)
    {
        logger.LogInformation((int)RequestCommand.StartGame,
            "Start game: [{RoomId:000}]", Room.Id);

        if (Room.Metadata.Mode == GameMode.Versus && Room.UserCount == 1 && !options.Value.AllowSoloInVersus)
        {
            return new StartGameEventData
            {
                Result = StartGameEventData.StartResult.InsufficientPlayers
            };
        }

        if (Room.UserCount > 1)
        {
            var slots = Room.Slots.OfType<Room.MemberSlot>().ToList();
            var counts = slots.Select(s => s.Team)
                .GroupBy(t => t)
                .ToDictionary(g => g.Key, g => g.Count());

            if (counts.Count == 1 || counts.Values.Max() - counts.Values.Min() != 0)
            {
                return new StartGameEventData
                {
                    Result = StartGameEventData.StartResult.TeamUnbalanced
                };
            }

            if (slots.Any(m => !m.IsReady))
            {
                return new StartGameEventData
                {
                    Result = StartGameEventData.StartResult.NotReady
                };
            }
        }

        Room.ScoreTracker.Reset();
        await Channel.Broadcast(new RoomStateChangedEventData
        {
            Number = Room.Id,
            State  = RoomState.Playing
        }, cancellationToken);

        await Room.Broadcast(Session, new StartGameEventData
        {
            Result = StartGameEventData.StartResult.Success
        }, cancellationToken);

        return new StartGameEventData
        {
            Result = StartGameEventData.StartResult.Success
        };
    }

    [CommandHandler(RequestCommand.Ready)]
    public async Task Ready(CancellationToken cancellationToken)
    {
        logger.LogInformation((int)RequestCommand.Ready,
            "Update room [{RoomId:000}] ready state", Room.Id);

        await Room.UpdateReadyState(Session, cancellationToken);
    }

    [CommandHandler(RequestCommand.ConfirmMusicLoaded)]
    public async Task ConfirmMusicLoaded(CancellationToken cancellationToken)
    {
        logger.LogInformation((int)RequestCommand.ConfirmMusicLoaded,
            "User music loaded: [{RoomId:000}]", Room.Id);

        var slots = Room.Slots.ToList();
        int memberId = slots.FindIndex(s => s is Room.MemberSlot m && m.Session == Session);
        if (memberId < 0)
            return;

        await Room.Broadcast(new MusicLoadedEventData
        {
            MemberId = (byte)memberId
        }, cancellationToken);
    }

    [CommandHandler(RequestCommand.ExitWaiting)]
    public async Task<ExitWaitingResponse> ExitRoom(CancellationToken cancellationToken)
    {
        logger.LogInformation(
            (int)RequestCommand.ExitWaiting,
            "Exit room: [{RoomId:000}]",
            Room.Id
        );

        var room = Room;
        await Session.Exit(room, cancellationToken);

        return new ExitWaitingResponse
        {
            Failed = false
        };
    }
}