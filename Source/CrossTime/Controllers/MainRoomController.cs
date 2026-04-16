using System.Diagnostics;
using CrossTime.Controllers.Filters;
using CrossTime.Messages;
using CrossTime.Messages.Events;
using CrossTime.Messages.Requests;
using CrossTime.Messages.Responses;
using Encore.Server;
using Microsoft.Extensions.Logging;
using Mozart.Data.Repositories;
using Mozart.Entities;
using Mozart.Events;
using Mozart.Metadata;
using Mozart.Services;
using Mozart.Sessions;

namespace CrossTime.Controllers;

[ChannelAuthorize]
public class MainRoomController(
    Session session,
    IUserRepository repository,
    IRoomService roomService,
    IEventPublisher<Room> publisher,
    ILogger<MainRoomController> logger
) : CommandController<Session>(session)
{
    private IChannel Channel => Session.Channel!;

    [CommandHandler(RequestCommand.GetCharacterInfo)]
    public async Task<CharacterInfoResponse> GetCharacterInfo(CancellationToken cancellationToken)
    {
        var actor = Session.GetAuthorizedToken<Actor>();
        logger.LogInformation((int)RequestCommand.GetCharacterInfo, "Get character info: [{User}]",
            actor.Nickname);

        await Session.WriteMessage(new SyncMembershipEventData
        {
            Gem             = actor.Gem,
            Point           = actor.Point,
            O2Cash          = 0,
            ItemCash        = 0,
            MusicCash       = 0,
            MembershipType  = actor.MembershipType
        }, cancellationToken);

        var giftBox = BuildGiftBoxResponse(actor);
        if (giftBox.Messages.Count > 0)
            await Session.WriteMessage(giftBox, cancellationToken);

        return new CharacterInfoResponse
        {
            Nickname   = actor.Nickname,
            Gender     = actor.Gender,
            Gem        = actor.Gem,
            Point      = actor.Point,
            Level      = actor.Level,
            Win        = actor.Win,
            Lose       = actor.Lose,
            Draw       = actor.Draw,
            Equipments = actor.Equipments,
            Inventory  = actor.Inventory.Select(i => (int)i.Id).ToList(),
            GemStar    = actor.GemStar,
            Ticket     = actor.Ticket
        };
    }

    [CommandHandler(RequestCommand.GetCharacterInfo)]
    public async Task GetMusicPremiumTimeList(CancellationToken cancellationToken)
    {
        var actor  = Session.Actor;
        var expiry = DateTime.MinValue;
        bool free  = gameOptions.Value.FreeMusic || Session.Actor.FreePass.Type == FreePassType.AllMusic;

        var acquiredMusic = new HashSet<int>();
        if (!free)
        {
            // TODO: Support time-limited promotion/event?
            if (Session.Actor.FreePass.Type != FreePassType.None)
                acquiredMusic = [..actor.AcquiredMusicIds.Select(i => (int)i)];

            // FreePass in the original server implementation may a lot more complex than this.
            // But we have no way to know how it works now.
            expiry = Session.Actor.FreePass.ExpiryDate;
        }
        else
        {
            acquiredMusic = [..Session.Channel!.GetMusicList()
                .Where(m => m.Value.IsPurchasable).Select(m => m.Key)];
        }

        await Session.WriteMessage(new MusicPremiumTimeEventData
        {
            Entries = Session.Channel!.GetMusicList()
                .Where(m =>
                    m.Value.IsPurchasable
                    && (free || (acquiredMusic.Contains(m.Key) || expiry != DateTime.MinValue))
                )
                .Select(m =>
                    new MusicPremiumTimeEventData.MusicEntry
                    {
                        MusicId = (ushort)m.Key,
                        Day     = (byte)(free || acquiredMusic.Contains(m.Key) ? 0 : expiry.Day),
                        Month   = (byte)(free || acquiredMusic.Contains(m.Key) ? 0 : expiry.Month),
                        Year    = (byte)(free || acquiredMusic.Contains(m.Key) ? 0 : expiry.Year % 1000),
                        Hour    = (byte)(free || acquiredMusic.Contains(m.Key) ? 0 : expiry.Hour),
                        Minute  = (byte)(free || acquiredMusic.Contains(m.Key) ? 0 : expiry.Minute)
                    }
                ).ToList()
        }, cancellationToken);
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

        Session.Actor.InstalledMusicIds = request.MusicIds.ToList();
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
                NoteCountHx = (ushort)music.NoteCountHx
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
                    UserIndexId = actor.UserId,
                    Username    = actor.Nickname, // Supposed to be username, but the server inject nickname anyway
                                                  // This may affect user list webpage function (see `CTuser_id`)
                    Nickname    = actor.Nickname,
                    Level       = actor.Level
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
                Mode             = room?.Mode ?? GameMode.Versus,
                Speed            = room?.Speed ?? GameSpeed.X10,
                Capacity         = (byte)(room?.Capacity ?? 0),
                UserCount        = (byte)(room?.UserCount ?? 0),
                MinLevelLimit    = (byte)(room?.MinLevelLimit ?? 0),
                MaxLevelLimit    = (byte)(room?.MaxLevelLimit ?? 0),
                Skills           = room?.Skills.ToList() ?? [],
                Premium          = false,
                Type             = (byte)(room?.Metadata?.Type ?? 0)
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

        var room = roomService.GetRoom(Channel, request.Number);
        if (room.Mode == GameMode.Single)
        {
            return new JoinRoomResponse
            {
                Result = JoinRoomResponse.JoinResult.InvalidMode
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
                Result       = JoinRoomResponse.JoinResult.Success,
                Index        = (byte)index,
                Team         = member.Team,
                RoomTitle    = room.Title,
                MusicId      = (ushort)room.MusicId,
                ArenaInfo    = new RoomArenaMessage(room.Arena, room.ArenaRandomSeed),
                Mode         = room.Mode,
                Difficulty   = room.Difficulty,
                Speed        = room.Speed,
                UserCount    = room.UserCount,
                Skills       = room.Skills.ToList(),
                Unknown      = false,
                TeamDisabled = !room.TeamEnabled,
                Slots        = room.Slots.Where(slot => (slot as Room.MemberSlot)?.Session != Session).Select((slot, i) =>
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
                                Gem             = m.Actor.Gem,
                                IsRoomMaster    = m.IsMaster,
                                Team            = m.Team,
                                Ready           = m.IsReady,
                                MusicState      = m.MusicState,
                                Equipments      = m.Actor.Equipments,
                                MusicIds        = m.Actor.InstalledMusicIds.ToList(),
                                CashPoint       = m.Actor.CashPoint,
                                FreePass        = m.Actor.FreePass.Type,
                                IsPlaying       = room.ScoreTracker.IsTracked(m.Session),
                                IsAdministrator = m.Actor.IsAdministrator
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
            "Create room: {Title} ({Mode})",
            request.Title, request.Mode
        );

        int type = 0;
        if (Session.Actor.IsAdministrator)
        {
            type = Session.Actor.Gender == Gender.Male ? Random.Shared.Next(5, 7)
                                                       : Random.Shared.Next(3, 5);
        }

        try
        {
            var room = roomService.CreateRoom(
                session:       Session,
                title:         request.Title,
                mode:          request.Mode,
                password:      request.HasPassword ? request.Password : string.Empty,
                minLevelLimit: request.MinLevelLimit,
                maxLevelLimit: request.MaxLevelLimit,
                premium:       request.Premium,
                type:          type
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
    [CommandHandler(RequestCommand.SyncWallet)]
    public async Task<SyncWalletResponse> SyncWallet(CancellationToken cancellationToken)
    {
        var actor = Session.Actor;
        var user  = (await repository.Find(actor.UserId, cancellationToken))!;

        actor.Sync(user);
        return new SyncWalletResponse
        {
            Gem     = actor.Gem,
            GemStar = actor.GemStar,
            Point   = actor.Point
        };
    }

    [CommandHandler(RequestCommand.GetGiftMessageList, ResponseCommand.GetGiftMessageList)]
    public async Task<GetGiftMessageListResponse> GetGiftMessages(CancellationToken cancellationToken)
    {
        var actor = Session.Actor;
        logger.LogInformation((int)RequestCommand.GetGiftMessageList,
            "Get user gift messages: [{User}]", actor.Nickname);

        var user = (await repository.Find(actor.UserId, cancellationToken))!;
        actor.Sync(user);

        return BuildGiftBoxResponse(actor);
    }

    [CommandHandler]
    public async Task ReadGiftMessage(ReadGiftMessageRequest request, CancellationToken cancellationToken)
    {
        var actor = Session.Actor;
        logger.LogInformation((int)RequestCommand.GetGiftMessageList,
            "Read gift message: [{User}] ({giftId})", actor.Nickname, request.GiftMessageId);

        var user    = (await repository.Find(actor.UserId, cancellationToken))!;
        var message = user.GiftMessages.FirstOrDefault(m => m.Id == request.GiftMessageId);
        if (message != null)
        {
            message.MarkAsRead();
            await repository.Update(user, cancellationToken);
            await repository.Commit(cancellationToken);
        }

        actor.Sync(user);
    }

    private GetGiftMessageListResponse BuildGiftBoxResponse(Actor actor)
    {
        return new GetGiftMessageListResponse
        {
            Messages = actor.GiftMessages.Select(m => new GetGiftMessageListResponse.GiftEntry
            {
                MessageId = m.Id,
                GiftType  = m.GiftType,
                WriteDate = m.WriteDate.ToShortDateString(),
                Sender    = m.SenderNickname,
                Title     = m.Title,
                Content   = m.Content,
            }).ToList()
        };
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
