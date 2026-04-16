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
    public void SetRoomMusic(SetRoomMusicRequest request)
    {
        logger.LogInformation(
            (int)RequestCommand.SetRoomMusic,
            "Update room [{RoomId:000}] music settings: [o2ma{MusicId} / Lv.{Level}]",
            Room.Id, request.MusicId, request.MissionLevel
        );

        Room.MusicId = request.MusicId;
        Room.Difficulty = request.Difficulty;
        Room.Speed = request.Speed;
        Room.SaveMetadataChanges(refresh: true);
    }

    [RoomMasterAuthorize]
    [CommandHandler]
    public void SetRoomAlbum(SetRoomAlbumRequest request)
    {
        logger.LogInformation(
            (int)RequestCommand.SetRoomAlbum,
            "Update room [{RoomId:000}] album settings: [{Speed} / {AlbumId}]",
            Room.Id, request.Speed, request.AlbumId
        );

        // Not supported: request does not include selected music ids
    }

    [CommandHandler]
    public async Task AcquireMusic(AcquireMusicRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            (int)RequestCommand.AcquireMusicRequest,
            "Room update [{RoomId:000}] acquire music",
            Room.Id
        );

        await Room.Broadcast(new AcquireMusicEventData
        {
            States = Room.Slots.Select((slot, i) =>
            {
                return slot switch
                {
                    Room.MemberSlot m => new AcquireMusicEventData.MemberMusicState
                    {
                        MemberId = (byte)i,
                        State    = m.MusicState == MusicState.NoMusic ? MusicState.Downloading : m.MusicState
                    },
                    _ => new AcquireMusicEventData.MemberMusicState
                    {
                        MemberId = byte.MaxValue,
                        State    = MusicState.NoMusic
                    },
                };
            }).Where(s => request.MemberIds.Contains(s.MemberId)).ToList()
        }, cancellationToken);
    }

    [CommandHandler]
    public async Task SyncMemberMusicState(SyncMemberMusicStateRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            (int)RequestCommand.SyncMemberMusicState,
            "Sync room member [{RoomId:000}] [{MemberId}] music state",
            Room.Id, request.MemberId
        );

        var member = (Room.MemberSlot)Room.Slots[request.MemberId];
        await Room.Broadcast(new SyncMemberMusicStateEventData
        {
            MemberId = request.MemberId,
            State    = member.MusicState
        }, cancellationToken);
    }

    [CommandHandler]
    public void UpdateMusicState(UpdateMusicStateRequest request)
    {
        var slots    = Room.Slots.ToList();
        int memberId = slots.FindIndex(s => s is Room.MemberSlot m && m.Session == Session);

        logger.LogInformation(
            (int)RequestCommand.UpdateMusicState,
            "Update room member [{RoomId:000}] [{MemberId}: {State}] music state",
            Room.Id, memberId, request.State
        );

        Room.UpdateMusicState(Session, request.State);
    }

    [CommandHandler]
    public async Task SetDownloadProgress(SetDownloadProgressRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            (int)RequestCommand.SetDownloadProgress,
            "Update room member [{RoomId:000}] [{MemberId}: {progress:00}%] download progress",
            Room.Id, request.MemberId, request.Progress
        );

        if (request.Progress >= 100)
        {
            var member = (Room.MemberSlot)Room.Slots[request.MemberId];
            member.MusicState = MusicState.Ready;
            member.Actor.InstalledMusicIds.Add((ushort)(Room.MusicId | 0x8000));
        }

        await Room.Broadcast(new DownloadProgressChangedEventData
        {
            MemberId   = request.MemberId,
            Percentage = request.Progress
        }, cancellationToken);
    }

    [RoomMasterAuthorize]
    [CommandHandler]
    public void SetRoomTitle(SetRoomTitleRequest request)
    {
        logger.LogInformation((int)RequestCommand.SetRoomTitle,
            "Update room [{RoomId:000}] title: [{Title}]", Room.Id, request.Title);

        Room.Title = request.Title;
        Room.SaveMetadataChanges();
    }

    [RoomMasterAuthorize]
    [CommandHandler]
    public void SetRoomArena(SetRoomArenaRequest request)
    {
        logger.LogInformation(
            (int)RequestCommand.SetRoomArena,
            "Update room [{RoomId:000}] arena: [{Arena}] ({Seed})",
            Room.Id, request.Payload.Arena, request.Payload.RandomSeed
        );

        Room.Arena           = request.Payload.Arena;
        Room.ArenaRandomSeed = request.Payload.RandomSeed;
        Room.SaveMetadataChanges();
    }

    [RoomMasterAuthorize]
    [CommandHandler]
    public void SetRoomSkill(SetRoomSkillRequest request)
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
    }

    [RoomMasterAuthorize]
    [CommandHandler]
    public void SetRoomMode(SetRoomModeRequest request)
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
    public async Task StartGame(CancellationToken cancellationToken)
    {
        logger.LogInformation((int)RequestCommand.StartGame,
            "Start game: [{RoomId:000}]", Room.Id);

        if (Room.Metadata.Mode == GameMode.Versus && Room.UserCount == 1 && !options.Value.AllowSoloInVersus)
        {
            await Session.WriteMessage(new StartGameEventData
            {
                Result = StartGameEventData.StartResult.InsufficientPlayers
            }, cancellationToken);
            return;
        }

        if (Room.UserCount > 1)
        {
            var slots = Room.Slots.OfType<Room.MemberSlot>().ToList();
            var counts = slots.Select(s => s.Team)
                .GroupBy(t => t)
                .ToDictionary(g => g.Key, g => g.Count());

            if (counts.Count == 1 || counts.Values.Max() - counts.Values.Min() != 0)
            {
                await Session.WriteMessage(new StartGameEventData
                {
                    Result = StartGameEventData.StartResult.TeamUnbalanced
                }, cancellationToken);

                return;
            }

            if (slots.Any(m => !m.IsReady))
            {
                await Session.WriteMessage(new StartGameEventData
                {
                    Result = StartGameEventData.StartResult.NotReady
                }, cancellationToken);

                return;
            }

            if (slots.Any(m => m.MusicState != MusicState.Ready))
            {
                await Room.Broadcast(new AcquireMusicEventData
                {
                    States = Room.Slots.Select((slot, i) =>
                    {
                        return slot switch
                        {
                            Room.MemberSlot m => new AcquireMusicEventData.MemberMusicState
                            {
                                MemberId = (byte)i,
                                State    = m.MusicState == MusicState.NoMusic ? MusicState.Downloading : m.MusicState
                            },
                            _ => new AcquireMusicEventData.MemberMusicState
                            {
                                MemberId = byte.MaxValue,
                                State    = MusicState.NoMusic
                            },
                        };
                    }).Where(s => s.MemberId != byte.MaxValue).ToList()
                }, cancellationToken);

                await Session.WriteMessage(new StartGameEventData
                {
                    Result = StartGameEventData.StartResult.None
                }, cancellationToken);

                return;
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
                await Session.WriteMessage(new StartGameEventData
                {
                    Result = StartGameEventData.StartResult.GenericError,
                }, cancellationToken);

                return;
            }

            await repository.Update(user, cancellationToken);
            pendingChanges = true;
        }

        bool freeMusic = Session.Channel!.FreeMusic ?? options.Value.FreeMusic;
        var memberUsers = new List<(Room.MemberSlot Member, User User)>();
        if (!freeMusic)
        {
            if (Session.Channel!.GetMusicList().TryGetValue(Room.MusicId, out var music)
                && music.IsPurchasable && (music.PriceO2Cash > 0 || music.PriceGem > 0))
            {
                var members = Room.Slots.OfType<Room.MemberSlot>().ToList();
                foreach (var member in members.Where(member => member.Actor.FreePass.Type == FreePassType.None))
                {
                    var memberUser = (await repository.Find(member.Actor.UserId, cancellationToken))!;
                    if (memberUser.CashPoint < 10)
                    {
                        await Session.WriteMessage(new StartGameEventData
                        {
                            Result = StartGameEventData.StartResult.GenericError,
                        }, cancellationToken);
                        return;
                    }

                    memberUser.CashPoint -= 10;
                    await repository.Update(memberUser, cancellationToken);
                    memberUsers.Add((member, memberUser));
                }
                pendingChanges = true;
            }
        }

        Room.StartGame();
        publisher.Monitor((ScoreTracker)Room.ScoreTracker);
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
            "Exit room: [{RoomId:000}]: {Reason}",
            Room.Id,
            request.Type
        );

        // Since reason payload can be forged easily, we need to add additional guard
        // TODO: Implement this handling in disconnection?
        if (request.Type == ExitWaitingRequest.ExitType.Penalized || (Room.ScoreTracker.IsTracked(Session) && Room.State == RoomState.Playing))
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
