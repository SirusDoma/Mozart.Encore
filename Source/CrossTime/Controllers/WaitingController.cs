using CrossTime.Controllers.Filters;
using CrossTime.Messages.Events;
using CrossTime.Messages.Requests;
using CrossTime.Messages.Responses;
using Encore.Server;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mozart.Data.Entities;
using Mozart.Data.Repositories;
using Mozart.Entities;
using Mozart.Events;
using Mozart.Metadata;
using Mozart.Options;
using Mozart.Services;
using Mozart.Sessions;

namespace CrossTime.Controllers;

[RoomAuthorize]
public class WaitingController(
    Session session,
    IUserRepository repository,
    IEventPublisher<ScoreTracker> publisher,
    IOptions<GameOptions> options,
    ILogger<WaitingController> logger
) : CommandController<Session>(session)
{
    private IRoom Room => Session.Room!;

    [RoomMasterAuthorize]
    [CommandHandler]
    public WaitingMusicChangedEventData SetRoomMusic(SetRoomMusicRequest request)
    {
        logger.LogInformation(
            (int)RequestCommand.SetRoomMusic,
            "Update room [{RoomId:000}] music settings: [o2ma{MusicId} / Lv.{Level}]",
            Room.Id, request.MusicId, request.MissionLevel
        );

        Room.MusicId = request.MusicId;
        Room.MissionLevel = request.MissionLevel;
        Room.SaveMetadataChanges();

        return new WaitingMusicChangedEventData
        {
            MusicId = request.MusicId,
            MissionLevel = request.MissionLevel
        };
    }

    [RoomMasterAuthorize]
    [CommandHandler]
    public WaitingAlbumChangedEventData SetRoomAlbum(SetRoomAlbumRequest request)
    {
        logger.LogInformation((int)RequestCommand.SetRoomMusic,
            "Update room [{RoomId:000}] album settings", Room.Id);

        // Not supported: request does not include selected music ids

        return new WaitingAlbumChangedEventData
        {
            AlbumId = Room.MusicId,
            Speed   = Room.Speed
        };
    }

    [CommandHandler]
    public MusicStateChangedEventData GetMusicState(GetMusicStateRequest request)
    {
        logger.LogInformation(
            (int)RequestCommand.GetMusicState,
            "Update room [{RoomId:000}] [{Member:00}] music state",
            Room.Id, request.MemberId
        );

        Room.UpdateMusicState(Session, request.MemberId);

        var member = Room.Slots.OfType<Room.MemberSlot>().Single(m => m.Session == Session);
        return new MusicStateChangedEventData
        {
            MemberId = request.MemberId,
            State    = member.MusicState
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
            Title = request.Title
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
    public WaitingSkillChangedEventData SetRoomSkill(SetRoomSkillRequest request)
    {
        logger.LogInformation(
            (int)RequestCommand.SetRoomSkill,
            "Update room [{RoomId:000}] skill settings: {Status}",
            Room.Id, request.Skills.Count == 0 || request.Skills is [<= 0] ? "inactive" : "active"
        );

        Room.Skills = request.Skills.Where(s => s > 0).ToList();
        Room.SkillsSeed = Room.Skills.Count > 0
            ? Random.Shared.Next(0, int.MaxValue) // TODO: Crack how seed actually used in client
            : 0;

        Room.SaveMetadataChanges();

        return new WaitingSkillChangedEventData
        {
            Skills = request.Skills
        };
    }

    [RoomMasterAuthorize]
    [CommandHandler]
    public WaitingModeChangedEventData SetRoomMode(SetRoomModeRequest request)
    {
        logger.LogInformation(
            (int)RequestCommand.SetRoomMusic,
            "Update room [{RoomId:000}] mode settings: {P1}",
            Room.Id, request.Mode
        );

        Room.Title = request.Title;
        Room.Mode = request.Mode;
        Room.Password = request.HasPassword == 1 ? request.Password : string.Empty;
        Room.SaveMetadataChanges();

        return new WaitingModeChangedEventData
        {
            Number       = request.Number,
            Title        = request.Title,
            Mode         = request.Mode,
            HasPassword  = request.HasPassword == 1,
            Password     = request.Password,
            TeamDisabled = !Room.TeamEnabled
        };
    }

    [CommandHandler(RequestCommand.ToggleTeamMode)]
    public void ToggleTeamMode()
    {
        logger.LogInformation((int)RequestCommand.SetRoomTeam,
            "Toggle team mode room [{RoomId:000}]", Room.Id);

        Room.TeamEnabled = !Room.TeamEnabled;
        Room.SaveMetadataChanges();
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

        var user = (await repository.Find(Session.Actor.UserId, cancellationToken))!;
        var skills = new List<int>();
        skills.AddRange(Room.Skills.Where(s => s != 0));

        if (skills.Count > 0)
        {
            for (int i = 0; i < user.Inventory.Capacity; i++)
            {
                var item = user.Inventory[i];
                if (skills.Any(s => item.Id == s))
                {
                    int rc = skills.RemoveAll(s => s == item.Id);
                    if (item.Count > 0)
                    {
                        user.Inventory[i] = new Inventory.BagItem
                        {
                            Id = item.Id,
                            Count = item.Count - 1
                        };
                    }
                    else
                    {
                        if (item.Count == 0)
                            skills.AddRange(Enumerable.Repeat((int)item.Id, rc)); // Something strange happened

                        user.Inventory[i] = Inventory.BagItem.Empty;
                    }
                }
            }

            if (skills.Count > 0)
            {
                // Host doesn't have insufficient attributive items in their inventory. Desync or forged?
                Room.SkillsSeed = 0;
            }
            else
            {
                await repository.Update(user, cancellationToken);
                await repository.Commit(cancellationToken);
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
