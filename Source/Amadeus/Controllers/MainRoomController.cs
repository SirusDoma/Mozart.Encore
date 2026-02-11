using System.Diagnostics;
using Amadeus.Controllers.Filters;
using Amadeus.Messages;
using Amadeus.Messages.Requests;
using Amadeus.Messages.Responses;
using Encore.Server;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mozart.Entities;
using Mozart.Events;
using Mozart.Metadata;
using Mozart.Options;
using Mozart.Services;
using Mozart.Sessions;

namespace Amadeus.Controllers;

[ChannelAuthorize]
public class MainRoomController(
    Session session,
    IOptions<GameOptions> gameOptions,
    IRoomService roomService,
    IEventPublisher<Room> publisher,
    ILogger<MainRoomController> logger
) : CommandController<Session>(session)
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
            Inventory          = actor.Inventory.Select(i => (int)i.Id).ToList(),
            AcquiredMusicIds   = Channel.FreeMusic ?? gameOptions.Value.FreeMusic
                ? Channel.GetMusicList()
                    .Values.Where(m => m.IsPurchasable)
                    .Select(m => (ushort)m.Id)
                    .ToList()
                : actor.AcquiredMusicIds,
            ItemGiftBox        = actor.GiftItems.Select(i =>
                new CharacterInfoResponse.GiftItemInfo
                {
                    GiftId = i.Id,
                    ItemId = i.ItemId,
                    Sender = i.SenderNickname
                }
            ).ToList(),
            MusicGiftBox        = actor.GiftMusics.Select(m =>
                new CharacterInfoResponse.GiftMusicInfo
                {
                    GiftId  = m.Id,
                    MusicId = m.MusicId,
                    Sender  = m.SenderNickname
                }
            ).ToList(),
            AttributiveItems   = actor.Inventory.Where(i => i.Count > 0).Select(i =>
                new CharacterInfoResponse.AttributiveItemInfo
                {
                    AttributiveItemId = i.Id,
                    ItemCount         = i.Count
                }
            ).ToList(),
        });
    }

    [CommandHandler]
    public void SendMusicList(MusicListRequest request)
    {
        logger.LogInformation((int)RequestCommand.SendMusicList,
            "Report client music list: {Count} music", request.MusicIds.Count);

        // For some stupid reasons, the most significant 4 bits of the music id is occupied with 0xF flag
        // So, o2ma100 become 0xF064
        //
        // However, DO NOT attempt to store the unflagged ids, because the format is being relied on other places.
        // If the server need to access to these ids for whatever reason, use the following code to unmask it:
        //
        //   var musicIds = new List<ushort>();
        //   foreach (ushort id in request.MusicIds)
        //   {
        //       // Using o2ma100 (0xF064) as example:
        //       int flag = id & 0xF000; // 0xF000
        //       int val  = id & 0x0FFF; // 0x64
        //
        //       // The rest is the logic from the client
        //       flag = flag > 0 ? flag << 16 : flag;
        //       ushort musicId = (ushort)(val | flag);
        //
        //       musicIds.Add(musicId);
        //   }
        //
        // The flag then used to mark the music with labels (e.g, "New" label)
        //
        // As you might aware, this imposes limitation on the maximum valid id (4096 or 0x1000 to be precise)
        // This is because the most significant 4 bits are rendered unusable
        //
        // Why this over a new field? Go figure yourself, but it is stupid regardless!

        Session.Actor.InstalledMusicIds = request.MusicIds;
    }

    [CommandHandler(RequestCommand.GetMusicList)]
    public MusicListResponse GetMusicList()
    {
        var musicList = Channel.GetMusicList();

        logger.LogInformation((int)RequestCommand.GetMusicList,
            "Get music list: [0/{channelId:00}] {Count} music", Channel.Id, musicList.Count);

        var musicInfoList = new List<MusicListResponse.MusicInfo>();
        foreach (var music in musicList.Values)
        {
            musicInfoList.Add(new MusicListResponse.MusicInfo
            {
                Id          = (ushort)music.Id,
                NoteCountEx = (ushort)music.NoteCountEx,
                NoteCountNx = (ushort)music.NoteCountNx,
                NoteCountHx = (ushort)music.NoteCountHx,
                Unknown     = 0 // Some sort of music flags. 0 most of the time, but it could be anything
            });
        }

        return new MusicListResponse
        {
            MusicList = musicInfoList
        };
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
                    Username = actor.Nickname, // Supposed to be username, but the server inject nickname anyway
                                               // This may affect user list webpage function (see `CTuser_id`)
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
                Number           = room?.Id ?? 0,
                State            = room?.State ?? new RoomState(),
                Title            = room?.Title ?? string.Empty,
                HasPassword      = !string.IsNullOrEmpty(room?.Password),
                MusicId          = (ushort)(room?.MusicId ?? 0),
                Difficulty       = room?.Difficulty ?? Difficulty.EX,
                Mode             = room?.Mode ?? GameMode.Single,
                Speed            = room?.Speed ?? GameSpeed.X10,
                Capacity         = (byte)(room?.Capacity ?? 0),
                UserCount        = (byte)(room?.UserCount ?? 0),
                MinLevelLimit    = (byte)(room?.MinLevelLimit ?? 0),
                MaxLevelLimit    = (byte)(room?.MaxLevelLimit ?? 0),
                AcquiredMusicIds = [] // Unused despite playing premium music
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
                MusicId    = (ushort)room.MusicId,
                ArenaInfo  = new RoomArenaMessage(room.Arena, (byte)room.ArenaRandomSeed),
                Mode       = room.Mode,
                Difficulty = room.Difficulty,
                Speed      = room.Speed,
                UserCount  = room.UserCount,
                Skills     = room.Skills.ToList(),
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
                                Nickname        = m.Actor.Nickname,
                                Level           = m.Actor.Level,
                                Gender          = m.Actor.Gender,
                                IsRoomMaster    = m.IsMaster,
                                Team            = m.Team,
                                Ready           = m.IsReady,
                                IsAdministrator = m.IsReady,
                                Equipments      = m.Actor.Equipments,
                                MusicIds        = m.Actor.InstalledMusicIds
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
