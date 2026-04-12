using Identity.Controllers.Filters;
using Identity.Messages.Events;
using Identity.Messages.Requests;
using Identity.Messages.Responses;
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

namespace Identity.Controllers;

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

    [CommandHandler(RequestCommand.GetMusicScoreList)]
    public MusicScoreListResponse GetMusicScoreList()
    {
        logger.LogInformation(
            (int)RequestCommand.GetMusicScoreList,
            "Get music score list"
        );

        return new MusicScoreListResponse
        {
            MaxScores = [
                new MusicScoreListResponse.MusicScoreEntry
                {
                    MusicId = 100,
                    Scores  = [1000, 1000, 1000],
                    Ranks   = [1, 5, 6]
                }
            ]
        };
    }

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
    public WaitingStateChangedEventData GetWaitingState(GetWaitingStateRequest request)
    {
        logger.LogInformation(
            (int)RequestCommand.GetWaitingState,
            "Update room [{RoomId:000}] [{Member:00}] waiting state",
            Room.Id, request.MemberId
        );

        Room.UpdateWaitingState(Session, request.MemberId);

        var member = Room.Slots.OfType<Room.MemberSlot>().Single(m => m.Session == Session);
        return new WaitingStateChangedEventData
        {
            MemberId = request.MemberId,
            State    = member.WaitingState
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
    public WaitingSkillChangedEventData SetRoomSkill(SetRoomSkillRequest request)
    {
        logger.LogInformation(
            (int)RequestCommand.SetRoomSkill,
            "Update room [{RoomId:000}] skill settings: {Status} ({count}: {skillId})",
            Room.Id, request.Skills.Count == 0 || request.Skills is [<= 0] ? "inactive" : "active", request.Skills.Count, request.Skills.FirstOrDefault()
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

        var slots = Room.Slots.OfType<Room.MemberSlot>().ToList();
        if (Room.UserCount > 1)
        {
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
                return new StartGameEventData
                {
                    Result = StartGameEventData.StartResult.GenericError,
                };
            }

            await repository.Update(user, cancellationToken);
            await repository.Commit(cancellationToken);
        }

        Session.Actor.Sync(user);
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

    [CommandHandler]
    public async Task<ExitWaitingResponse> ExitRoom(ExitWaitingRequest request, CancellationToken cancellationToken)
    {
        var actor = Session.Actor;
        logger.LogInformation(
            (int)RequestCommand.ExitWaiting,
            "Exit room: [{RoomId:000}]: {Reason:000}",
            Room.Id,
            request.Reason
        );

        // Since reason payload can be forged easily, we need to add additional guard
        // TODO: Implement this handling in disconnection?
        if (request.Reason == 1 || (Room.ScoreTracker.IsTracked(Session) && Room.State == RoomState.Playing))
        {
            var user = (await repository.Find(actor.UserId, cancellationToken))!;
            user.PenaltyCount += 1;
            user.PenaltyLevel = user.PenaltyCount switch
            {
                <= 3  => 0,
                <= 6  => 1,
                <= 9  => 2,
                <= 12 => 3,
                <= 15 => 4,
                <= 18 => 5,
                <= 21 => 6,
                >= 22 => 7
            };

            await repository.Update(user, cancellationToken);
            await repository.Commit(cancellationToken);
        }

        var room = Room;
        Session.Exit(room);

        return new ExitWaitingResponse
        {
            Failed = false
        };
    }
}
