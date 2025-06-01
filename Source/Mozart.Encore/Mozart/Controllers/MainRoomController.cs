using Encore.Messaging;
using Encore.Server;
using Encore.Sessions;

namespace Mozart;

[Authorize]
public class MainRoomController(Session session, IMessageCodec codec) : CommandController(session)
{
    [CommandHandler(RequestCommand.GetCharacterInfo)]
    public Task<CharacterInfoResponse> GetCharacterInfo(CancellationToken cancellationToken)
    {
        return Task.FromResult(new CharacterInfoResponse
        {
            DisableInventory = false,
            CharacterInfo    = Session.GetAuthorizedToken<CharacterInfo>()
        });
    }

    [CommandHandler]
    public Task SendMusicList(MusicListRequest request, CancellationToken cancellationToken)
    {
        var character = Session.GetAuthorizedToken<CharacterInfo>();
        character.MusicIds = request.MusicIds;

        return Task.CompletedTask;
    }

    [CommandHandler(RequestCommand.GetChannelInfo)]
    [CommandHandler(RequestCommand.GetUserList)]
    public Task<UserListResponse> GetUserList(CancellationToken cancellationToken)
    {
        return Task.FromResult(new UserListResponse
        {
            Users =
            [
                new ()
                {
                    Level = 100,
                    Nickname = "Mozart"
                },
                new ()
                {
                    Level = 10,
                    Nickname = "Random guy #1"
                }
            ]
        });
    }

    [CommandHandler(RequestCommand.GetChannelInfo)]
    public Task<RoomListResponse> GetRoomList(CancellationToken cancellationToken)
    {
        return Task.FromResult(new RoomListResponse
        {
            Rooms =
            [
                new RoomListResponse.RoomInfo
                {
                    Number        = 7,
                    State         = RoomState.Waiting,
                    Title         = "Leave me alone",
                    HasPassword   = true,
                    MusicId       = 102,
                    Mode          = GameMode.Versus,
                    Difficulty    = Difficulty.HX,
                    Speed         = GameSpeed.X40,
                    PlayerCount   = 1,
                    MinLevelLimit = 50,
                    MaxLevelLimit = 100
                },
                new RoomListResponse.RoomInfo
                {
                    Number = 1,
                    State = RoomState.Playing,
                    Title = "Go somewhere else",
                    HasPassword = false,
                    MusicId = 107,
                    Mode = GameMode.Versus,
                    Difficulty = Difficulty.EX,
                    Speed = GameSpeed.X25,
                    PlayerCount = 2
                }
            ]
        });
    }

    [CommandHandler]
    public Task<JoinRoomResponse> JoinRoom(JoinRoomRequest request, CancellationToken cancellationToken)
    {
        var character = Session.GetAuthorizedToken<CharacterInfo>();
        return Task.FromResult(new JoinRoomResponse
        {
            Result      = JoinRoomResponse.JoinResult.Success,
            Index       = 0,
            Team        = RoomTeam.B,
            RoomTitle   = "Leave me alone",
            MusicId     = 102,
            ArenaInfo   = new RoomArenaMessage(Arena.HonkyTonky),
            Mode        = GameMode.Versus, // Joining Jam mode will soft-lock you inside the gameplay screen.
            Difficulty  = Difficulty.NX,
            Speed       = GameSpeed.X15,
            PlayerCount = 2,
            Slots       =
            [
                new JoinRoomResponse.RoomSlotInfo
                {
                    Index      = 0,
                    State      = JoinRoomResponse.SlotState.Occupied,
                    MemberInfo = new JoinRoomResponse.RoomMemberInfo()
                    {
                        Nickname     = character.Nickname,
                        Level        = character.Level,
                        Gender       = character.Gender,
                        IsRoomMaster = false,
                        Team         = RoomTeam.B,
                        Ready        = false,
                        Equipments   = character.Equipments,
                        MusicIds     = character.MusicIds
                    }
                },
                new JoinRoomResponse.RoomSlotInfo()
                {
                    Index = 1,
                    State = JoinRoomResponse.SlotState.Occupied,
                    MemberInfo = new JoinRoomResponse.RoomMemberInfo()
                    {
                        Nickname     = "The master",
                        Level        = 999,
                        Gender       = Gender.Female,
                        IsRoomMaster = true,
                        Team         = RoomTeam.A,
                        Ready        = false,
                        Equipments   = new Dictionary<ItemType, int> {
                            [ItemType.Instrument]         = 039,
                            [ItemType.Hair]               = 004,
                            [ItemType.Earring]            = 000,
                            [ItemType.Gloves]             = 000,
                            [ItemType.Accessories]        = 000,
                            [ItemType.Top]                = 107,
                            [ItemType.Pants]              = 131,
                            [ItemType.Glasses]            = 000,
                            [ItemType.Necklace]           = 000,
                            [ItemType.ClothesAccessories] = 000,
                            [ItemType.Shoes]              = 171,
                            [ItemType.Face]               = 036
                        },
                        MusicIds     = character.MusicIds
                    }
                },
                new JoinRoomResponse.RoomSlotInfo()
                {
                    Index = 2,
                    State = JoinRoomResponse.SlotState.Unoccupied,
                },
                new JoinRoomResponse.RoomSlotInfo()
                {
                    Index = 3,
                    State = JoinRoomResponse.SlotState.Unoccupied,
                },
                new JoinRoomResponse.RoomSlotInfo()
                {
                    Index = 4,
                    State = JoinRoomResponse.SlotState.Unoccupied,
                },
                new JoinRoomResponse.RoomSlotInfo()
                {
                    Index = 5,
                    State = JoinRoomResponse.SlotState.Unoccupied,
                },
                new JoinRoomResponse.RoomSlotInfo()
                {
                    Index = 6,
                    State = JoinRoomResponse.SlotState.Unoccupied,
                },
                new JoinRoomResponse.RoomSlotInfo()
                {
                    Index = 7,
                    State = JoinRoomResponse.SlotState.Unoccupied,
                }
            ]
        });
    }

    [CommandHandler]
    public Task<CreateRoomResponse> CreateRoom(CreateRoomRequest request, CancellationToken cancellationToken)
    {
        Task.Run(async () =>
        {
            var character = Session.GetAuthorizedToken<CharacterInfo>();

            await Task.Delay(500, cancellationToken);
            await Session.WriteFrameAsync(codec.Encode(new PlayerJoinWaitingEventData
            {
                MemberId   = 1,
                Nickname   = "The guest",
                Level      = 999,
                Gender     = Gender.Female,
                Team       = RoomTeam.B,
                Ready      = true,
                Equipments = new Dictionary<ItemType, int>
                {
                    [ItemType.Instrument]         = 039,
                    [ItemType.Hair]               = 004,
                    [ItemType.Earring]            = 000,
                    [ItemType.Gloves]             = 000,
                    [ItemType.Accessories]        = 000,
                    [ItemType.Top]                = 107,
                    [ItemType.Pants]              = 131,
                    [ItemType.Glasses]            = 000,
                    [ItemType.Necklace]           = 000,
                    [ItemType.ClothesAccessories] = 000,
                    [ItemType.Shoes]              = 171,
                    [ItemType.Face]               = 036
                },
                MusicIds = character.MusicIds
            }), cancellationToken);
        }, cancellationToken);

        // Broadcast: RoomCreatedEventData and RoomInfoChangedEventData
        return Task.FromResult(new CreateRoomResponse
        {
            Result = CreateRoomResponse.CreateResult.Success,
            Number = 005
        });
    }

    [CommandHandler(RequestCommand.ChannelLogout)]
    public ChannelLogoutResponse ChannelLogout()
    {
        return new ChannelLogoutResponse();
    }
}