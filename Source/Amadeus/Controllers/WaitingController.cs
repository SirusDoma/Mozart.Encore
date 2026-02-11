using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Encore.Server;

using Amadeus.Controllers.Filters;
using Mozart.Entities;
using Mozart.Events;
using Amadeus.Messages;
using Amadeus.Messages.Events;
using Amadeus.Messages.Events.Waiting;
using Amadeus.Messages.Requests;
using Amadeus.Messages.Responses;
using Mozart.Metadata;
using Mozart.Options;
using Mozart.Services;
using Mozart.Sessions;

namespace Amadeus.Controllers;

[RoomAuthorize]
public class WaitingController(Session session, IEventPublisher<ScoreTracker> publisher,
    IOptions<GameOptions> options, ILogger<WaitingController> logger) : CommandController<Session>(session)
{
    private IRoom Room => Session.Room!;

    [RoomMasterAuthorize]
    [CommandHandler]
    public WaitingMusicChangedEventData SetRoomMusic(SetRoomMusicRequest request)
    {
        logger.LogInformation(
            (int)RequestCommand.SetRoomMusic,
            "Update room [{RoomId:000}] music settings: [{Difficulty} / {Speed} / o2ma{MusicId}]",
            Room.Id, request.Difficulty, request.Speed, request.MusicId
        );

        Room.MusicId = request.MusicId;
        Room.Difficulty = request.Difficulty;
        Room.Speed = request.Speed;
        Room.SaveMetadataChanges();

        return new WaitingMusicChangedEventData
        {
            MusicId    = request.MusicId,
            Difficulty = request.Difficulty,
            Speed      = request.Speed
        };
    }

    [RoomMasterAuthorize]
    [CommandHandler]
    public WaitingAlbumChangedEventData SetRoomAlbum(SetRoomAlbumRequest request)
    {
        logger.LogInformation(
            (int)RequestCommand.SetRoomMusic,
            "Update room [{RoomId:000}] album settings: [{Speed} / {AlbumId}]",
            Room.Id, request.Speed, request.AlbumId
        );

        Room.MusicId = request.AlbumId;
        Room.Speed = request.Speed;
        Room.SaveMetadataChanges();

        return new WaitingAlbumChangedEventData
        {
            AlbumId = request.AlbumId,
            Speed = request.Speed
        };
    }

    [CommandHandler]
    public async Task<UserAlbumEligibilityChangedEventData> CheckUserAlbumEligibility(
        CheckUserAlbumEligibilityRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            (int)RequestCommand.SetRoomArena,
            "Update room [{RoomId:000}] [{Member:00}] album eligibility status",
            Room.Id, request.MemberId
        );

        await Room.Broadcast(Session, new UserAlbumEligibilityChangedEventData
        {
            MemberId = request.MemberId,
            Ineligible = false
        }, cancellationToken);

        return new UserAlbumEligibilityChangedEventData
        {
            MemberId   = request.MemberId,
            Ineligible = false
        };
    }

    [RoomMasterAuthorize]
    [CommandHandler]
    public WaitingRoomTitleEventData SetRoomTitle(SetRoomTitleRequest request)
    {
        logger.LogInformation((int)RequestCommand.SetRoomTitle,
            "Update room [{RoomId:000}] title: [{Title}]", Room.Id, request.Title);

        Room.Title = request.Title;
        Room.SaveMetadataChanges();

        return new WaitingRoomTitleEventData
        {
            Title = request.Title[..Math.Min(21, request.Title.Length)]
        };
    }

    [RoomMasterAuthorize]
    [CommandHandler]
    public WaitingArenaChangedEventData SetRoomArena(SetRoomArenaRequest request)
    {
        logger.LogInformation(
            (int)RequestCommand.SetRoomArena,
            "Update room [{RoomId:000}] arena: [{Arena}] ({Seed})",
            Room.Id, request.Payload.Arena, request.Payload.RandomSeed
        );

        Room.Arena           = request.Payload.Arena;
        Room.ArenaRandomSeed = request.Payload.RandomSeed;
        Room.SaveMetadataChanges();

        return new WaitingArenaChangedEventData
        {
            Arena      = request.Payload.Arena,
            RandomSeed = request.Payload.RandomSeed
        };
    }

    [RoomMasterAuthorize]
    [CommandHandler]
    public async Task SetRoomSkill(SetRoomSkillRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            (int)RequestCommand.SetRoomMusic,
            "Update room [{RoomId:000}] skill settings: {Status}",
            Room.Id, request.Skills.Count == 0 || request.Skills is [<= 0] ? "inactive" : "active"
        );

        Room.Skills     = request.Skills;
        Room.SkillsSeed = Random.Shared.Next(0, int.MaxValue); // TODO: Crack how seed actually used in client
        Room.SaveMetadataChanges();

        await Session.WriteMessage(new WaitingSkillChangedEventData
        {
            Skills = request.Skills
        }, cancellationToken);
    }

    [CommandHandler]
    public void SetRoomPlayerTeam(SetTeamRequest request)
    {
        logger.LogInformation((int)RequestCommand.SetRoomTeam,
            "Update room [{RoomId:000}] [{User}] team: [{Team}]", Room.Id, Session.Actor.Nickname, request.Team);

        Room.UpdateTeam(Session, request.Team);
    }

    [CommandHandler]
    public void SetPlayerInstrument(SetInstrumentRequest request)
    {
        // Do not reply to this command:
        // The game was supposed to send "InstrumentId" but instead, it always sends us 0
        // return new PlayerInstrumentChangedEventData()
        // {
        //     MemberId     = 0,
        //     InstrumentId = request.InstrumentId
        // };
    }

    [RoomMasterAuthorize]
    [CommandHandler]
    public void UpdateSlot(UpdateSlotRequest request)
    {
        logger.LogInformation((int)RequestCommand.UpdateSlot,
            "Update room [{RoomId:000}] slot: [{MemberId}]", Room.Id, request.MemberId);

        Room.UpdateSlot(Session, request.MemberId);
    }

    [CommandHandler(RequestCommand.StartGame)]
    [RoomMasterAuthorize]
    public StartGameEventData StartGame()
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

        Room.StartGame();
        publisher.Monitor((ScoreTracker)Room.ScoreTracker);

        return new StartGameEventData
        {
            Result = StartGameEventData.StartResult.Success,
            SkillsSeed = Room.SkillsSeed
        };
    }

    [CommandHandler(RequestCommand.Ready)]
    public void Ready()
    {
        logger.LogInformation((int)RequestCommand.Ready,
            "Update room [{RoomId:000}] ready state", Room.Id);

        Room.UpdateReadyState(Session);
    }

    [CommandHandler(RequestCommand.ExitWaiting)]
    public ExitWaitingResponse ExitRoom()
    {
        logger.LogInformation(
            (int)RequestCommand.ExitWaiting,
            "Exit room: [{RoomId:000}]",
            Room.Id
        );

        var room = Room;
        Session.Exit(room);

        return new ExitWaitingResponse
        {
            Failed = false
        };
    }
}
