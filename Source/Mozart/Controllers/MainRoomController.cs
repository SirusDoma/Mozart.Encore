using System.Diagnostics;
using Microsoft.Extensions.Logging;

using Encore.Server;

using Mozart.Controllers.Filters;
using Mozart.Entities;
using Mozart.Events;
using Mozart.Messages;
using Mozart.Messages.Requests;
using Mozart.Messages.Responses;
using Mozart.Metadata;
using Mozart.Services;
using Mozart.Sessions;

namespace Mozart.Controllers;

[ChannelAuthorize]
public class MainRoomController(Session session,IRoomService roomService, IEventPublisher<Room> publisher,
    ILogger<MainRoomController> logger) : CommandController<Session>(session)
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

        var rooms  = roomService.GetRooms(Channel);
        var states = new List<RoomListResponse.RoomInfo>();

        for (ushort i = 0; i < Channel.Capacity; i++)
        {
            var room = rooms.SingleOrDefault(r => r.Id == i);
            states.Add(new RoomListResponse.RoomInfo
            {
                Number         = room?.Id ?? 0,
                State          = room?.State ?? new RoomState(),
                Title          = room?.Title ?? string.Empty,
                HasPassword    = !string.IsNullOrEmpty(room?.Password),
                MusicId        = room?.MusicId ?? 0,
                Difficulty     = room?.Difficulty ?? Difficulty.EX,
                Mode           = room?.Mode ?? GameMode.Single,
                Speed          = room?.Speed ?? GameSpeed.X10,
                Capacity       = (byte)(room?.Capacity ?? 0),
                UserCount      = (byte)(room?.UserCount ?? 0),
                MinLevelLimit  = (byte)(room?.MinLevelLimit ?? 0),
                MaxLevelLimit  = (byte)(room?.MaxLevelLimit ?? 0)
            });
        }

        return new RoomListResponse
        {
            Rooms = states
        };
    }

    [CommandHandler]
    public JoinRoomResponse JoinRoom(JoinRoomRequest request)
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

            Session.Register(room);
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
    public CreateRoomResponse CreateRoom(CreateRoomRequest request)
    {
        logger.LogInformation(
            (int)RequestCommand.CreateRoom,
            "Create room: {title}",
            request.Title
        );

        try
        {
            var room = roomService.CreateRoom(
                session:       Session,
                title:         request.Title,
                mode:          request.Mode,
                password:      request.HasPassword ? request.Password : string.Empty,
                minLevelLimit: request.MinLevelLimit,
                maxLevelLimit: request.MaxLevelLimit
            );
            publisher.Monitor(room);

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
    public ChannelLogoutResponse ChannelLogout()
    {
        logger.LogInformation(
            (int)RequestCommand.ChannelLogout,
            "Channel logout: [{channelId:00}]",
            Channel.Id
        );

        Session.Exit(Channel);
        return new ChannelLogoutResponse();
    }
}