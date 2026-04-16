using Encore.Sessions;
using Mozart.Metadata;
using Mozart.Metadata.Room;
using Mozart.Options;
using Mozart.Services;
using Mozart.Sessions;
using Session = Mozart.Sessions.Session;

namespace Mozart.Entities;


public class Room : Broadcastable, IRoom
{
    public const byte MaxCapacity = 8;

    private RoomMetadata _previous;

    private readonly IRoomService _service;
    private readonly RoomMetadata _metadata;
    private readonly GameOptions _options;

    private readonly List<ISlot> _slots;

    public Room(IRoomService service, Session master, RoomMetadata metadata, GameOptions options)
    {
        _service  = service;
        _previous = (RoomMetadata)metadata.Clone();
        _metadata = metadata;
        _options  = options;
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

        public bool IsMaster { get; set; }

        public bool IsReady { get; set; }

        public MusicState MusicState { get; set; } = MusicState.None;

        public Actor Actor => Session.GetAuthorizedToken<Actor>();
    }

    public int Id => _metadata.Id;

    public IChannel Channel { get; }

    public RoomState State => _metadata.State;

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

    public string Password
    {
        get => _metadata.Password;
        set => _metadata.Password = value;
    }

    public int MusicId
    {
        get => _metadata.MusicId;
        set => _metadata.MusicId = value;
    }


    public int MissionLevel
    {
        get => _metadata.MissionLevel;
        set => _metadata.MissionLevel = value;
    }

    public GameMode Mode
    {
        get => _metadata.Mode;
        set => _metadata.Mode = value;
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

    public IList<int> Skills
    {
        get => _metadata.Skills;
        set => _metadata.Skills = value;
    }

    public int SkillsSeed
    {
        get => _metadata.SkillsSeed;
        set => _metadata.SkillsSeed = value;
    }

    public bool TeamEnabled
    {
        get => _metadata.TeamEnabled;
        set => _metadata.TeamEnabled = value;
    }

    public Session Master => _slots.OfType<MemberSlot>().Single(s => s.IsMaster).Session;

    public IReadOnlyList<ISlot> Slots => _slots;

    public IScoreTracker ScoreTracker { get; private set; }

    public event EventHandler<RoomUserJoinedEventArgs>? UserJoined;
    public event EventHandler<RoomUserLeftEventArgs>? UserLeft;
    public event EventHandler<RoomUserLeftEventArgs>? UserDisconnected;
    public event EventHandler<RoomUserTeamChangedEventArgs>? UserTeamChanged;
    public event EventHandler<RoomUserMusicStateChangedEventArgs>? UserMusicStateChanged;
    public event EventHandler<RoomUserReadyStateChangedEventArgs>? UserReadyStateChanged;

    public event EventHandler<RoomMusicChangedEventArgs>? MusicChanged;
    public event EventHandler<RoomAlbumChangedEventArgs>? AlbumChanged;
    public event EventHandler<RoomArenaChangedEventArgs>? ArenaChanged;
    public event EventHandler<RoomStateChangedEventArgs>? StateChanged;
    public event EventHandler<RoomSlotChangedEventArgs>? SlotChanged;
    public event EventHandler<RoomSkillChangedEventArgs>? SkillChanged;
    public event EventHandler<RoomModeChangedEventArgs>? ModeChanged;
    public event EventHandler<RoomTeamToggleChangedEventArgs>? TeamToggleChanged;

    public override IReadOnlyList<Session> Sessions
        => _slots.OfType<MemberSlot>().Select(m => m.Session).ToList();

    public event EventHandler<SessionEventArgs>? SessionDisconnected;

    void IRoom.Register(Session session)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(session.Authorized, true, nameof(session));
        
        if (_slots.OfType<MemberSlot>().Any(m => m.Session == session))
            return;

        if (session.Room != this)
        {
            session.Register(this);
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

            UserJoined?.Invoke(this, new RoomUserJoinedEventArgs
            {
                MemberId = i,
                Member   = member
            });

            return;
        }

        throw new InvalidOperationException("Room is full");
    }

    void IRoom.Remove(Session session)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(session.Authorized, true, nameof(session));

        if (session.Room != null)
        {
            if (session.Room != this)
                throw new ArgumentOutOfRangeException(nameof(session));

            session.Exit(this);
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
            {
                var master = ((MemberSlot)_slots[masterId]);
                master.IsMaster = true;
                master.IsReady  = true;
            }
        }

        if (State == RoomState.Playing)
            ScoreTracker.Untrack(session);

        UserLeft?.Invoke(this, new RoomUserLeftEventArgs
        {
            MemberId           = index,
            Member             = member,
            RoomMasterMemberId = masterId
        });

        if (!_slots.OfType<MemberSlot>().Any())
            _service.DeleteRoom(Channel, Id);
    }

    public bool IsMember(Session session)
    {
        return _slots.Any(s => s is MemberSlot m && m.Session == session);
    }

    public bool IsMember(Actor actor)
    {
        return _slots.Any(s => s is MemberSlot m && m.Actor == actor);
    }

    public void SaveMetadataChanges()
    {
        if (_previous.MusicId != _metadata.MusicId ||
            _previous.MissionLevel != _metadata.MissionLevel ||
            _previous.Difficulty != _metadata.Difficulty ||
            _previous.Speed != _metadata.Speed)
        {
            if (_metadata.Mode == GameMode.Jam)
            {
                AlbumChanged?.Invoke(this, new RoomAlbumChangedEventArgs
                {
                    AlbumId = _metadata.MusicId,
                    Speed  = _metadata.Speed
                });
            }
            else
            {
                MusicChanged?.Invoke(this, new RoomMusicChangedEventArgs
                {
                    MusicId = _metadata.MusicId,
                    MissionLevel = _metadata.MissionLevel,
                    Difficulty = _metadata.Difficulty,
                    Speed = _metadata.Speed,
                });
            }
        }

        if (_previous.Arena != _metadata.Arena ||
            _previous.ArenaRandomSeed != _metadata.ArenaRandomSeed)
        {
            ArenaChanged?.Invoke(this, new RoomArenaChangedEventArgs
            {
                Arena      = _metadata.Arena,
                RandomSeed = _metadata.ArenaRandomSeed
            });
        }

        if (_previous.State != _metadata.State)
        {
            StateChanged?.Invoke(this, new RoomStateChangedEventArgs
            {
                PreviousState = _previous.State,
                CurrentState  = _metadata.State
            });
        }

        if (!_previous.Skills.Equals(_metadata.Skills))
        {
            SkillChanged?.Invoke(this, new RoomSkillChangedEventArgs
            {
                Skills = _metadata.Skills
            });
        }

        if (_previous.Mode != _metadata.Mode ||
            _previous.Title != _metadata.Title ||
            _previous.Password != _metadata.Password)
        {
            ModeChanged?.Invoke(this, new RoomModeChangedEventArgs
            {
                Title       = _metadata.Title,
                Mode        = _metadata.Mode,
                Password    = _metadata.Password,
                TeamEnabled = _metadata.TeamEnabled
            });
        }

        if (_previous.TeamEnabled != _metadata.TeamEnabled)
        {
            TeamToggleChanged?.Invoke(this, new RoomTeamToggleChangedEventArgs
            {
                Enabled = _metadata.TeamEnabled
            });
        }

        // Capture state
        _previous = (RoomMetadata)_metadata.Clone();
    }

    public void UpdateReadyState(Session session)
    {
        int index = _slots.FindIndex(s => s is MemberSlot m && m.Session == session);
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(session)); // request forged?

        var member     = (_slots[index] as MemberSlot)!;
        member.IsReady = !member.IsReady;

        UserReadyStateChanged?.Invoke(this, new RoomUserReadyStateChangedEventArgs
        {
            MemberId = index,
            Member   = member,
            Ready    = member.IsReady
        });
    }

    public void UpdateTeam(Session session, RoomTeam team)
    {
        int index = _slots.FindIndex(s => s is MemberSlot m && m.Session == session);
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(session)); // request forged?

        var member   = (_slots[index] as MemberSlot)!;
        member.Team = team;

        UserTeamChanged?.Invoke(this, new RoomUserTeamChangedEventArgs
        {
            MemberId = index,
            Member   = member,
            Team     = member.Team
        });
    }

    public void UpdateMusicState(Session session, int memberId)
    {
        if (memberId is < 0 or >= MaxCapacity)
            throw new ArgumentOutOfRangeException(nameof(memberId));

        if (_slots[memberId] is not MemberSlot member)
            throw new ArgumentOutOfRangeException(nameof(memberId));

        var state = MusicState.None;

        if (!_options.FreeMission)
        {
            if (Channel.GetMusicList().TryGetValue(MusicId, out _)
                && member.Actor.MembershipType == 0
                && !member.Actor.AcquiredMusicIds.Contains((ushort)MusicId))
            {
                state = MusicState.NoAccess;
            }
        }

        member.MusicState = state;

        UserMusicStateChanged?.Invoke(this, new RoomUserMusicStateChangedEventArgs
        {
            MemberId = memberId,
            Member   = member,
            State    = state
        });
    }

    public void UpdateSlot(Session session, int slotId)
    {
        if (session != Master)
            throw new ArgumentOutOfRangeException(nameof(session)); // request forged?

        if (slotId is < 0 or >= MaxCapacity)
            throw new ArgumentOutOfRangeException(nameof(slotId));

        var target = _slots[slotId];
        var result = RoomSlotActionType.PlayerKicked;

        switch (target)
        {
            case MemberSlot:
                _slots[slotId] = new VacantSlot();
                result = RoomSlotActionType.PlayerKicked;

                break;
            case LockedSlot:
                _slots[slotId] = new VacantSlot();
                result = RoomSlotActionType.SlotUnlocked;

                break;
            case VacantSlot:
                _slots[slotId] = new LockedSlot();
                result = RoomSlotActionType.SlotLocked;

                break;
        }

        SlotChanged?.Invoke(this, new RoomSlotChangedEventArgs
        {
            SlotId       = slotId,
            PreviousSlot = target,
            CurrentSlot  = _slots[slotId],
            ActionType   = result,
            Capacity     = Capacity,
            UserCount    = UserCount
        });

        if (target is MemberSlot member)
            member.Session.Exit(this);
    }

    public void StartGame()
    {
        ScoreTracker = new ScoreTracker(this);

        _metadata.State = RoomState.Playing;
        SaveMetadataChanges();

        _ = ScheduleStartTimeout();
    }

    public void CompleteGame()
    {
        if (!ScoreTracker.Completed || _metadata.State != RoomState.Playing)
            return;

        _metadata.State = RoomState.Waiting;
        SaveMetadataChanges();
    }

    public void Disconnect(Session session)
    {
        IRoom room = this;
        room.Remove(session);

        SessionDisconnected?.Invoke(this, new SessionEventArgs { Session = session });
    }

    public override void Invalidate()
    {
        foreach (var session in Sessions)
        {
            if (session.Connected)
                continue;

            session.Exit(Channel);
        }
    }

    private async Task ScheduleStartTimeout()
    {
        if (_options.MusicLoadTimeout <= 0)
            return;

        await Task.Delay(TimeSpan.FromSeconds(_options.MusicLoadTimeout));

        if (State != RoomState.Playing || ScoreTracker.Count == UserCount)
            return;

        for (int i = 0; i < _slots.Count; i++)
        {
            var slot = _slots[i];
            if (slot is not MemberSlot member)
                continue;

            if (ScoreTracker.IsTracked(member.Session))
                continue;

            member.Session.Exit(this);
            UserDisconnected?.Invoke(this, new RoomUserLeftEventArgs
            {
                MemberId           = i,
                Member             = member,
                RoomMasterMemberId = _slots.FindIndex(s => s is MemberSlot { IsMaster: true })
            });
        }
    }
}

