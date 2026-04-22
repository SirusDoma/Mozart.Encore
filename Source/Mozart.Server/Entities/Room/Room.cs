using Encore.Sessions;
using Mozart.Data.Entities;
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

        if (GameMode == GameMode.Live)
        {
            _slots =
            [
                new MemberSlot
                {
                    Session  = master,
                    Team     = RoomTeam.A,
                    LiveRole = RoomLiveRole.Champion,
                    IsMaster = true,
                    IsReady  = true
                },
                new LockedSlot(),
                new LockedSlot(),
                new VacantSlot(),
                new VacantSlot(),
                new VacantSlot(),
                new VacantSlot(),
                new VacantSlot(),
            ];
        }
        else
        {
            _slots =
            [
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
        }

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

        public RoomLiveRole LiveRole { get; set; }

        public int WinStreak { get; set; }

        public bool IsMaster { get; set; }

        public bool IsReady { get; set; }

        public MusicState MusicState { get; set; } = MusicState.Ready;

        public PlayingState PlayingState { get; set; } = PlayingState.None;

        public Actor Actor => Session.GetAuthorizedToken<Actor>();
    }

    public int Id => _metadata.Id;

    public IChannel Channel { get; }

    public RoomState State => _metadata.State;

    public RoomMetadata Metadata => _metadata;

    public int Capacity => Slots.Count(s => s is not LockedSlot);

    public int UserCount => Slots.Count(s => s is MemberSlot);

    public int PlayingUserCount => Slots.Count(s => s is MemberSlot { PlayingState: PlayingState.Playing });

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

    public KeyMode KeyMode
    {
        get => _metadata.KeyMode;
        set => _metadata.KeyMode = value;
    }

    public GameMode GameMode
    {
        get => _metadata.GameMode;
        set => _metadata.GameMode = value;
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

    public int MinLevelLimit
    {
        get => _metadata.MinLevelLimit;
        set => _metadata.MinLevelLimit = value;
    }

    public int MaxLevelLimit
    {
        get => _metadata.MaxLevelLimit;
        set => _metadata.MaxLevelLimit = value;
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

    public bool Premium => _metadata.Premium;

    public bool IsSelectingMusic { get; set; }
    public bool IsRelaySessionCreated { get; set; }

    public Session Master => _slots.OfType<MemberSlot>().Single(s => s.IsMaster).Session;

    public IReadOnlyList<ISlot> Slots => _slots;

    public IScoreTracker ScoreTracker { get; private set; }

    public event EventHandler<RoomUserJoinedEventArgs>? UserJoined;
    public event EventHandler<RoomUserLeftEventArgs>? UserLeft;
    public event EventHandler<RoomUserLeftEventArgs>? UserDisconnected;
    public event EventHandler<RoomUserTeamChangedEventArgs>? UserTeamChanged;
    public event EventHandler<RoomUserMusicStateChangedEventArgs>? UserMusicStateChanged;
    public event EventHandler<RoomUserReadyStateChangedEventArgs>? UserReadyStateChanged;

    public event EventHandler<RoomTitleChangedEventArgs>? TitleChanged;
    public event EventHandler<RoomMusicChangedEventArgs>? MusicChanged;
    public event EventHandler<RoomAlbumChangedEventArgs>? AlbumChanged;
    public event EventHandler<RoomArenaChangedEventArgs>? ArenaChanged;
    public event EventHandler<RoomStateChangedEventArgs>? StateChanged;
    public event EventHandler<RoomSlotChangedEventArgs>? SlotChanged;
    public event EventHandler<RoomSkillChangedEventArgs>? SkillChanged;

    public override IReadOnlyList<Session> Sessions
        => _slots.OfType<MemberSlot>().Select(m => m.Session).ToList();

    public event EventHandler<SessionEventArgs<TcpSession>>? SessionDisconnected;

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
            Session      = session,
            Team         = Enum.GetValues<RoomTeam>().Except(_slots.OfType<MemberSlot>().Select(m => m.Team)).First(),
            IsMaster     = false,
            IsReady      = false,
            PlayingState = State == RoomState.Playing ? PlayingState.Waiting : PlayingState.None
        };

        for (int i = 0; i < _slots.Count; i++)
        {
            if (_slots[i] is not VacantSlot)
                continue;

            _slots[i] = member;
            if (i == 3)
                member.LiveRole = RoomLiveRole.Challenger;

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

        if (GameMode == GameMode.Live)
        {
            // DO NOT update master id after the re-arrangement

            var queueMembers = new List<MemberSlot>();
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i] is not MemberSlot m)
                    continue;

                if (index is 0 or 3 && _slots[index] is not MemberSlot)
                    _slots[index] = m;
                else if (i >= 4)
                    queueMembers.Add(m);

                _slots[i] = new VacantSlot();
            }

            int placed = 0;
            for (int i = 4; i < _slots.Count && placed < queueMembers.Count; i++)
            {
                if (_slots[i] is LockedSlot)
                    continue;

                _slots[i] = queueMembers[placed++];
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

    public void SaveMetadataChanges(bool refresh = false)
    {
        if (_previous.Title != _metadata.Title || _previous.Password != _metadata.Password)
        {
            TitleChanged?.Invoke(this, new RoomTitleChangedEventArgs
            {
                Title    = _metadata.Title,
                Password = _metadata.Password
            });
        }

        if (refresh ||
            _previous.MusicId != _metadata.MusicId ||
            _previous.Difficulty != _metadata.Difficulty ||
            _previous.Speed != _metadata.Speed)
        {
            if (_metadata.GameMode == GameMode.Jam)
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

        var member  = (_slots[index] as MemberSlot)!;
        member.Team = team;

        UserTeamChanged?.Invoke(this, new RoomUserTeamChangedEventArgs
        {
            MemberId = index,
            Member   = member,
            Team     = member.Team
        });
    }

    public void UpdateMusicState(Session session, MusicState state)
    {
        int index = _slots.FindIndex(s => s is MemberSlot m && m.Session == session);
        if (_slots[index] is not MemberSlot member)
            throw new ArgumentOutOfRangeException(nameof(state));

        switch (Channel.FreeMusic ?? _options.FreeMusic)
        {
            case true when state is MusicState.NoAccess:
                state = MusicState.Ready;
                break;
            case false when state == MusicState.Ready:
            {
                if (Channel.GetMusicList().TryGetValue(MusicId, out var music)
                    && music is { IsPurchasable: true, PriceO2Cash: > 0 }
                    && !member.Actor.AcquiredMusicIds.Contains((ushort)MusicId)
                    && member.Actor.FreePass.Type == FreePassType.None
                    && member.Actor.CashPoint < 10)
                {
                    state = MusicState.NoAccess;
                }

                break;
            }
        }

        member.MusicState = state;
        UserMusicStateChanged?.Invoke(this, new RoomUserMusicStateChangedEventArgs
        {
            MemberId = index,
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
        if (GameMode == GameMode.Live && (slotId == 1 || slotId == 2))
        {
            _slots[slotId] = new LockedSlot();
            result = RoomSlotActionType.SlotLocked;
        }
        else
        {
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

    public void UpdateSlotPositions(bool newChampion)
    {
        if (GameMode != GameMode.Live || _slots[0] is not MemberSlot champion || _slots[3] is not MemberSlot challenger)
            return;

        if (newChampion)
        {
            foreach (var member in _slots.OfType<MemberSlot>())
            {
                member.IsMaster = false;
                member.IsReady  = false;
            }

            champion.WinStreak   = 0;
            challenger.LiveRole  = RoomLiveRole.Champion;
            challenger.WinStreak = 0;
            _slots[0]            = challenger;

            challenger.IsMaster = true;
            challenger.IsReady  = false;
        }
        else
            champion.WinStreak++;

        var loser = !newChampion ? challenger : champion;
        _slots[3]  = new VacantSlot();

        bool challengerPromoted = false;
        for (int i = 4; i < _slots.Count; i++)
        {
            if (_slots[i] is not MemberSlot queued)
                continue;

            queued.LiveRole     = RoomLiveRole.Challenger;
            _slots[3]           = queued;
            _slots[i]           = new VacantSlot();
            challengerPromoted  = true;
            break;
        }

        if (!challengerPromoted)
        {
            loser.LiveRole = RoomLiveRole.Challenger;
            _slots[3]      = loser;
            return;
        }

        loser.LiveRole = RoomLiveRole.Spectator;
        for (int i = _slots.Count - 1; i >= 4; i--)
        {
            if (_slots[i] is not VacantSlot)
                continue;

            _slots[i] = loser;
            break;
        }

        var queueMembers = new List<MemberSlot>();
        for (int i = 4; i < _slots.Count; i++)
        {
            if (_slots[i] is not MemberSlot member)
                continue;

            queueMembers.Add(member);
            _slots[i] = new VacantSlot();
        }

        int placed = 0;
        for (int i = 4; i < _slots.Count && placed < queueMembers.Count; i++)
        {
            if (_slots[i] is LockedSlot)
                continue;

            _slots[i] = queueMembers[placed++];
        }
    }

    public void StartGame()
    {
        ScoreTracker = new ScoreTracker(this);
        IsRelaySessionCreated = false;

        foreach (var member in _slots.OfType<MemberSlot>())
            member.PlayingState = PlayingState.Playing;

        _metadata.State = RoomState.Playing;
        SaveMetadataChanges();

        _ = ScheduleStartTimeout();
    }

    public void CompleteGame()
    {
        if (!ScoreTracker.Completed || _metadata.State != RoomState.Playing)
            return;

        foreach (var member in _slots.OfType<MemberSlot>())
        {
            member.IsReady = member.IsMaster;
            member.PlayingState = PlayingState.None;

            if (member.IsMaster)
                _metadata.Title = $"{member.WinStreak} Wins : {member.Actor.Nickname}";
        }

        _metadata.State = RoomState.Waiting;

        SaveMetadataChanges();
    }

    public void Disconnect(Session session)
    {
        IRoom room = this;
        room.Remove(session);

        SessionDisconnected?.Invoke(this, new SessionEventArgs<TcpSession> { Session = session });
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

        if (State != RoomState.Playing || ScoreTracker.Count == PlayingUserCount)
            return;

        for (int i = 0; i < _slots.Count; i++)
        {
            var slot = _slots[i];
            if (slot is not MemberSlot member)
                continue;

            if (member.PlayingState == PlayingState.Waiting)
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

