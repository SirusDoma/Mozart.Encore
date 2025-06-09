using Mozart.Messages;
using Mozart.Messages.Events;
using Mozart.Messages.Responses;
using Mozart.Services;
using Mozart.Sessions;

namespace Mozart.Entities;

public interface IRoom : IBroadcastable
{
    int Id { get; }
    RoomState State { get; }
    RoomMetadata Metadata { get; }
    int Capacity { get; }
    int UserCount { get; }
    string Title { get; set; }
    int MusicId { get; set; }
    Difficulty Difficulty { get; set; }
    GameSpeed Speed { get; set; }
    Arena Arena { get; set; }
    byte ArenaRandomSeed { get; set; }
    Actor Master { get; }
    IReadOnlyList<Room.ISlot> Slots { get; }
    IScoreTracker ScoreTracker { get; }


    ValueTask Register(Session session, CancellationToken cancellationToken);
    ValueTask Remove(Session session, CancellationToken cancellationToken);
    ValueTask SaveMetadataChanges(Session initiator, CancellationToken cancellationToken);

    ValueTask UpdateReadyState(Session session, CancellationToken cancellationToken);
    ValueTask UpdateTeam(Session session, RoomTeam team, CancellationToken cancellationToken);
    ValueTask UpdateSlot(Session session, int memberId, CancellationToken cancellationToken);

    ValueTask Disconnect(Session session, CancellationToken cancellationToken);
}

public class Room : Broadcastable, IRoom
{
    public const byte MaxCapacity = 8;

    private readonly IRoomService _service;

    private RoomMetadata _previous;
    private readonly RoomMetadata _metadata;

    private readonly List<ISlot> _slots;

    public Room(IRoomService service, Session master, RoomMetadata metadata)
    {
        _service = service;
        _previous = metadata;
        _metadata = metadata;
        _slots = [
            new MemberSlot
            {
                Session  = master,
                Team     = RoomTeam.A,
                IsMaster = true,
                IsReady  = true
            },
            new VacantSlot(),
            new VacantSlot(),
            new VacantSlot(),
            new VacantSlot(),
            new VacantSlot(),
            new VacantSlot(),
            new VacantSlot(),
        ];

        Channel = master.Channel!;
        ScoreTracker = new ScoreTracker(this);
    }

    public interface ISlot;

    public class VacantSlot : ISlot;

    public class LockedSlot : ISlot;

    public class MemberSlot : ISlot
    {
        public required Session Session { get; init; }

        public required RoomTeam Team { get; set; }

        public bool IsMaster { get; set; } = false;

        public bool IsReady { get; set; } = false;

        public Actor Actor => Session.GetAuthorizedToken<Actor>();

    }

    public int Id => _metadata.Id;

    public IChannel Channel { get; }

    public RoomState State => _metadata.State;

    public GameMode Mode => _metadata.Mode;

    public string Password => _metadata.Password;

    public int MinLevelLimit => _metadata.MinLevelLimit;

    public int MaxLevelLimit => _metadata.MaxLevelLimit;

    public RoomMetadata Metadata => _metadata;

    public int Capacity => Slots.Count(s => s is not LockedSlot);

    public int UserCount => Slots.Count(s => s is MemberSlot);

    public string Title
    {
        get => _metadata.Title;
        set => _metadata.Title = value;
    }

    public int MusicId
    {
        get => _metadata.MusicId;
        set => _metadata.MusicId = value;
    }

    public Difficulty Difficulty
    {
        get => _metadata.Difficulty;
        set => _metadata.Difficulty = value;
    }

    public GameSpeed Speed
    {
        get => _metadata.Speed;
        set => _metadata.Speed = value;
    }

    public Arena Arena
    {
        get => _metadata.Arena;
        set => _metadata.Arena = value;
    }

    public byte ArenaRandomSeed
    {
        get => _metadata.ArenaRandomSeed;
        set => _metadata.ArenaRandomSeed = value;
    }

    public Actor Master => _slots.OfType<MemberSlot>().Single(s => s.IsMaster).Actor;

    public IReadOnlyList<ISlot> Slots => _slots;

    public IScoreTracker ScoreTracker { get; }

    public override IReadOnlyList<Session> Sessions
        => _slots.OfType<MemberSlot>().Select(m => m.Session).ToList();

    public event EventHandler<Encore.Sessions.SessionEventArgs>? SessionDisconnected;

    async ValueTask IRoom.Register(Session session, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(session.Authorized, true, nameof(session));

        if (_slots.OfType<MemberSlot>().Any(m => m.Session == session))
            return;

        if (session.Room != this)
        {
            await session.Register(this, cancellationToken);
            return;
        }

        var member = new MemberSlot
        {
            Session  = session,
            Team     = Enum.GetValues<RoomTeam>().Except(_slots.OfType<MemberSlot>().Select(m => m.Team)).First(),
            IsMaster = false,
            IsReady  = false
        };

        for (int i = 0; i < _slots.Count; i++)
        {
            if (_slots[i] is not VacantSlot)
                continue;

            _slots[i] = member;
            await Broadcast(session, new UserJoinWaitingEventData
            {
                MemberId   = (byte)i,
                Nickname   = member.Actor.Nickname,
                Level      = member.Actor.Level,
                Gender     = member.Actor.Gender,
                Team       = member.Team,
                Ready      = member.IsReady,
                Equipments = member.Actor.Equipments,
                MusicIds   = member.Actor.MusicIds
            }, cancellationToken);

            return;
        }

        throw new InvalidOperationException("Room is full");
    }

    async ValueTask IRoom.Remove(Session session, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(session.Authorized, true, nameof(session));

        if (session.Room != null)
        {
            if (session.Room != this)
                throw new ArgumentOutOfRangeException(nameof(session));

            await session.Exit(this, cancellationToken);
            return;
        }

        int index = _slots.FindIndex(s => s is MemberSlot m && m.Session == session);
        if (index < 0)
            return; // Might be kick left-over state

        var member   = (_slots[index] as MemberSlot)!;
        int masterId = _slots.FindIndex(s => s is MemberSlot { IsMaster: true });

        _slots[index] = new VacantSlot();
        if (member.IsMaster)
        {
            masterId = _slots.FindIndex(s => s is MemberSlot m && m.Session != session);
            if (masterId >= 0)
                ((MemberSlot)_slots[masterId]).IsMaster = true;
        }

        await ScoreTracker.Untrack(session, cancellationToken);
        await Broadcast(session, new UserLeaveWaitingEventData
        {
            MemberId              = (byte)index,
            NewRoomMasterMemberId = (byte)masterId,
        }, cancellationToken);

        if (!_slots.OfType<MemberSlot>().Any())
            await _service.DeleteRoom(Channel, Id, cancellationToken);
    }


    public async ValueTask SaveMetadataChanges(Session initiator, CancellationToken cancellationToken)
    {
        if (_previous.Title != _metadata.Title)
        {
            await initiator.Channel!.Broadcast(initiator, new RoomTitleChangedEventData
            {
                Number = _metadata.Id,
                Title  = _metadata.Title
            }, cancellationToken);

            await Broadcast(initiator, new WaitingRoomTitleEventData
            {
                Title  = _metadata.Title
            }, cancellationToken);

        }

        if (_previous.MusicId != _metadata.MusicId ||
            _previous.Difficulty != _metadata.Difficulty ||
            _previous.Speed != _metadata.Speed)
        {
            await initiator.Channel!.Broadcast(initiator, new RoomMusicChangedEventData
            {
                Number     = _metadata.Id,
                MusicId    = _metadata.MusicId,
                Difficulty = _metadata.Difficulty,
                Speed      = _metadata.Speed,

            }, cancellationToken);

            await Broadcast(initiator, new WaitingMusicChangedEventData
            {
                MusicId    = _metadata.MusicId,
                Difficulty = _metadata.Difficulty,
                Speed      = _metadata.Speed,

            }, cancellationToken);
        }

        if (_previous.Arena != _metadata.Arena ||
            _previous.ArenaRandomSeed != _metadata.ArenaRandomSeed)
        {
            await Broadcast(initiator, new RoomArenaChangedEventData
            {
                Arena      = _metadata.Arena,
                RandomSeed = _metadata.ArenaRandomSeed
            }, cancellationToken);
        }

        if (_previous.State != _metadata.State)
        {
            await initiator.Channel!.Broadcast(initiator, new RoomStateChangedEventData
            {
                Number = _metadata.Id,
                State  = _metadata.State
            }, cancellationToken);

            if (_previous.State == RoomState.Waiting)
            {
                await Broadcast(initiator, new StartGameEventData
                {
                    Result = StartGameEventData.StartResult.Success
                }, cancellationToken);
            }
        }

        _previous = new RoomMetadata
        {
            Id              = _metadata.Id,
            Title           = _metadata.Title,
            Mode            = _metadata.Mode,
            MusicId         = _metadata.MusicId,
            Difficulty      = _metadata.Difficulty,
            Speed           = _metadata.Speed,
            MinLevelLimit   = _metadata.MinLevelLimit,
            MaxLevelLimit   = _metadata.MaxLevelLimit,
            Arena           = _metadata.Arena,
            ArenaRandomSeed = _metadata.ArenaRandomSeed,
            Password        = _metadata.Password,
            State           = _metadata.State
        };
    }

    public async ValueTask UpdateReadyState(Session session, CancellationToken cancellationToken)
    {
        int index = _slots.FindIndex(s => s is MemberSlot m && m.Session == session);
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(session)); // request forged?

        var member     = (_slots[index] as MemberSlot)!;
        member.IsReady = !member.IsReady;

        await Broadcast(new MemberReadyStateChangedEventData()
        {
            MemberId = (byte)index,
            Ready    = member.IsReady
        }, cancellationToken);
    }

    public async ValueTask UpdateTeam(Session session, RoomTeam team, CancellationToken cancellationToken)
    {
        int index = _slots.FindIndex(s => s is MemberSlot m && m.Session == session);
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(session)); // request forged?

        var member   = (_slots[index] as MemberSlot)!;
        member.Team = team;

        await Broadcast(new MemberTeamChangedEventData
        {
            MemberId = (byte)index,
            Team     = team
        }, cancellationToken);
    }

    public async ValueTask UpdateSlot(Session session, int memberId,
        CancellationToken cancellationToken)
    {
        if (session.Actor != Master)
            throw new ArgumentOutOfRangeException(nameof(session)); // request forged?

        if (memberId is < 0 or >= MaxCapacity)
            throw new ArgumentOutOfRangeException(nameof(memberId));

        var target = _slots[memberId];
        var result = RoomSlotUpdateEventData.EventType.PlayerKicked;
        switch (target)
        {
            case MemberSlot member:
                _slots[memberId] = new VacantSlot();

                await member.Session.WriteMessage(new KickEventData(), cancellationToken);
                result = RoomSlotUpdateEventData.EventType.PlayerKicked;

                break;
            case LockedSlot:
                _slots[memberId] = new VacantSlot();
                result = RoomSlotUpdateEventData.EventType.SlotUnlocked;

                break;
            case VacantSlot:
                _slots[memberId] = new LockedSlot();
                result = RoomSlotUpdateEventData.EventType.SlotLocked;

                break;
        }

        await Broadcast(new RoomSlotUpdateEventData()
        {
            Index = (byte)memberId,
            Type = result
        }, cancellationToken);

        await session.Channel!.Broadcast(new RoomUserCountChangedEventData
        {
            Number    = Id,
            Capacity  = (byte)Capacity,
            UserCount = (byte)UserCount
        }, cancellationToken);
    }

    public async ValueTask Disconnect(Session session, CancellationToken cancellationToken)
    {
        IRoom room = this;
        await room.Remove(session, CancellationToken.None);

        SessionDisconnected?.Invoke(this, new Encore.Sessions.SessionEventArgs { Session = session });
    }


    protected override IEnumerable<Session> GetSessionsByContext<TContext>(TContext ctx)
    {
        return [];
    }
}

public class RoomMetadata
{
    public required int Id { get; init; }

    public required string Title { get; set; }

    public required GameMode Mode { get; set; }

    public required int MusicId { get; set; }

    public required Difficulty Difficulty { get; set; }

    public required GameSpeed Speed { get; set; }

    public required int MinLevelLimit { get; init; }

    public required int MaxLevelLimit { get; init; }

    public required Arena Arena { get; set; }

    public byte ArenaRandomSeed { get; set; } = 0;

    public string Password { get; init; } = string.Empty;

    public RoomState State { get; set; } = RoomState.Waiting;
}




