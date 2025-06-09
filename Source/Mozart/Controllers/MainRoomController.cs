using System.Diagnostics;
using Microsoft.Extensions.Logging;

using Encore.Server;

using Mozart.Controllers.Filters;
using Mozart.Entities;
using Mozart.Messages;
using Mozart.Messages.Requests;
using Mozart.Messages.Responses;
using Mozart.Services;
using Mozart.Sessions;

namespace Mozart.Controllers;

[ChannelAuthorize]
public class MainRoomController(Session session,IRoomService roomService, ILogger<MainRoomController> logger)
    : CommandController<Session>(session)
{
    private IChannel Channel => Session.Channel!;

    [CommandHandler(RequestCommand.GetCharacterInfo)]
    public Task<CharacterInfoResponse> GetCharacterInfo(CancellationToken cancellationToken)
    {
        var actor = Session.GetAuthorizedToken<Actor>();
        logger.LogInformation((int)RequestCommand.GetCharacterInfo, "Get character info: [{User}]",
            actor.Nickname);

        return Task.FromResult(new CharacterInfoResponse
        {
            DisableInventory   = false,
            Nickname           = actor.Nickname,
            Gender             = actor.Gender,
            Gem                = actor.Gem,
            Point              = actor.Point,
            Level              = actor.Level,
            Win                = actor.Win,
            Lose               = actor.Lose,
            Draw               = actor.Draw,
            Experience         = actor.Experience,
            IsAdministrator    = actor.IsAdministrator,
            Equipments         = actor.Equipments,
            Inventory          = actor.Inventory,
            AttributiveItemIds = actor.AttributiveItemIds,
        });
    }

    [CommandHandler]
    public void SendMusicList(MusicListRequest request)
    {
        logger.LogInformation((int)RequestCommand.SendMusicList,
            "Report client music list: {Count} music", request.MusicIds.Count);

        var actor = Session.GetAuthorizedToken<Actor>();
        actor.MusicIds = request.MusicIds;
    }

    [CommandHandler(RequestCommand.GetChannelInfo)]
    [CommandHandler(RequestCommand.GetUserList)]
    public UserListResponse GetUserList()
    {
        logger.LogInformation(
            (int)RequestCommand.GetUserList,
            "Get user list: [0/{channelId:00}]",
            Channel.Id
        );

        return new UserListResponse
        {
            Users = Channel.Sessions.Select(e =>
            {
                var actor = e.GetAuthorizedToken<Actor>();
                return new UserListResponse.UserInfo
                {
                    Level    = actor.Level,
                    Nickname = actor.Nickname
                };
            }).ToList()
        };
    }

    [CommandHandler(RequestCommand.GetChannelInfo)]
    public RoomListResponse GetRoomList()
    {
        logger.LogInformation(
            (int)RequestCommand.GetChannelInfo,
            "Get room list: [0/{channelId:00}]",
            Channel.Id
        );

        return new RoomListResponse
        {
            Rooms = roomService.GetRooms(Channel).Select(room => new RoomListResponse.RoomInfo
            {
                Number         = room.Id,
                State          = room.State,
                Title          = room.Title,
                HasPassword    = !string.IsNullOrEmpty(room.Password),
                MusicId        = room.MusicId,
                Difficulty     = room.Difficulty,
                Mode           = room.Mode,
                Speed          = room.Speed,
                Capacity       = (byte)room.Capacity,
                UserCount      = (byte)room.UserCount,
                MinLevelLimit  = (byte)room.MinLevelLimit,
                MaxLevelLimit  = (byte)room.MaxLevelLimit
            }).ToList()
        };
    }

    [CommandHandler]
    public async Task<JoinRoomResponse> JoinRoom(JoinRoomRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            (int)RequestCommand.JoinWaiting,
            "Join room: [{RoomId:000}]",
            request.RoomNumber
        );

        var room = roomService.GetRoom(Channel, request.RoomNumber);
        if (room.Mode == GameMode.Single)
        {
            return new JoinRoomResponse
            {
                Result = JoinRoomResponse.JoinResult.InvalidMode
            };
        }

        if (room.State != RoomState.Waiting)
        {
            return new JoinRoomResponse
            {
                Result = JoinRoomResponse.JoinResult.InProgress
            };
        }

        if (room.Mode == GameMode.Jam)
        {
            logger.LogWarning(
                (int)RequestCommand.JoinWaiting,
                "  Joining unsupported Jam Mode room."
            );
        }

        try
        {
            if (!string.IsNullOrEmpty(room.Password) && room.Password != request.Password)
                throw new ArgumentOutOfRangeException(nameof(request));

            await Session.Register(room, cancellationToken);
            int index  = room.Slots.ToList().FindIndex(r => r is Room.MemberSlot m && m.Session == Session);
            var member = (room.Slots[index] as Room.MemberSlot)!;

            return new JoinRoomResponse
            {
                Result     = JoinRoomResponse.JoinResult.Success,
                Index      = (byte)index,
                Team       = member.Team,
                RoomTitle  = room.Title,
                MusicId    = room.MusicId,
                ArenaInfo  = new RoomArenaMessage(room.Arena, (byte)room.ArenaRandomSeed),
                Mode       = room.Mode,
                Difficulty = room.Difficulty,
                Speed      = room.Speed,
                UserCount  = room.UserCount,
                Slots      = room.Slots.Select((slot, i) =>
                {
                    return slot switch
                    {
                        Room.VacantSlot => new JoinRoomResponse.RoomSlotInfo
                        {
                            Index = (byte)i,
                            State = JoinRoomResponse.RoomSlotState.Unoccupied
                        },
                        Room.LockedSlot => new JoinRoomResponse.RoomSlotInfo
                        {
                            Index = (byte)i,
                            State = JoinRoomResponse.RoomSlotState.Locked
                        },
                        Room.MemberSlot m => new JoinRoomResponse.RoomSlotInfo
                        {
                            Index = (byte)i,
                            State = JoinRoomResponse.RoomSlotState.Occupied,
                            MemberInfo = new JoinRoomResponse.RoomMemberInfo
                            {
                                Nickname     = m.Actor.Nickname,
                                Level        = m.Actor.Level,
                                Gender       = m.Actor.Gender,
                                IsRoomMaster = m.IsMaster,
                                Team         = m.Team,
                                Ready        = m.IsReady,
                                Equipments   = m.Actor.Equipments,
                                MusicIds     = m.Actor.MusicIds
                            }
                        },
                        _ => throw new UnreachableException()
                    };
                }).ToList()
            };
        }
        catch (InvalidOperationException)
        {
            return new JoinRoomResponse
            {
                Result = JoinRoomResponse.JoinResult.Full
            };
        }
        catch (ArgumentOutOfRangeException ex) when (ex.ParamName == "request")
        {
            return new JoinRoomResponse
            {
                Result = JoinRoomResponse.JoinResult.InvalidPassword
            };
        }
    }

    [CommandHandler]
    public async Task<CreateRoomResponse> CreateRoom(CreateRoomRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            (int)RequestCommand.CreateRoom,
            "Create room: {title}",
            request.Title
        );

        try
        {
            var room = await roomService.CreateRoom(Session, request, cancellationToken);
            return new CreateRoomResponse
            {
                Result = CreateRoomResponse.CreateResult.Success,
                Number = room.Id
            };
        }
        catch (InvalidOperationException)
        {
            return new CreateRoomResponse
            {
                Result = CreateRoomResponse.CreateResult.ChannelFull,
                Number = 0
            };
        }
    }

    [CommandHandler(RequestCommand.ChannelLogout)]
    public async Task<ChannelLogoutResponse> ChannelLogout(CancellationToken cancellationToken)
    {
        logger.LogInformation(
            (int)RequestCommand.ChannelLogout,
            "Channel logout: [{channelId:00}]",
            Channel.Id
        );

        await Session.Exit(Channel, cancellationToken);
        return new ChannelLogoutResponse();
    }
}