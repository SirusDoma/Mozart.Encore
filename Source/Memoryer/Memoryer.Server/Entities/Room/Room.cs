using Encore.Data.Entities;
using Encore.Metadata;
using Encore.Metadata.Room;
using Encore.Server.Sessions;
using Mozart.Metadata;
using Mozart.Metadata.Room;
using Mozart.Options;
using Mozart.Services;

namespace Mozart.Entities;


public class Room : Encore.Entities.Room, IRoom
{
    private RoomState _previousState;
    private Changes _changes;

    private readonly IRoomService _service;
    private readonly RoomMetadata _metadata;
    private readonly GameOptions _options;

    public Room(IRoomService service, Session master, RoomMetadata metadata, GameOptions options)
        : base(master)
    {
        _service  = service;
        _previousState = metadata.State;
        _metadata = metadata;
        _options  = options;

        if (GameMode == GameMode.Live)
        {
            _slots.Clear();
            _slots.AddRange(
            [
                new MemberSlot
                {
                    Session  = master,
                    Team     = RoomTeam.A,
                    Role = MemberRole.Champion,
                    IsMaster = true,
                    IsReady  = true
                },
                new LockedSlot(),
                new LockedSlot(),
                new VacantSlot(),
                new VacantSlot(),
                new VacantSlot(),
                new VacantSlot(),
                new VacantSlot()
            ]);
        }

        ScoreTracker = new ScoreTracker(this);
    }

    [Flags]
    private enum Changes
    {
        None  = 0,
        Params = 1 << 0,
        Music = 1 << 1,
        Arena = 1 << 2,
        State = 1 << 3,
        Skills = 1 << 4
    }

    public int Id => _metadata.Id;

    public RoomState State => _metadata.State;

    public RoomMetadata Metadata => _metadata;

    public string Title
    {
        get => _metadata.Title;
        set { _metadata.Title = value; _changes |= Changes.Params; }
    }

    public string Password
    {
        get => _metadata.Password;
        set { _metadata.Password = value; _changes |= Changes.Params; }
    }

    public KeyMode KeyMode
    {
        get => _metadata.KeyMode;
        set { _metadata.KeyMode = value; _changes |= Changes.Params; }
    }

    public GameMode GameMode
    {
        get => _metadata.GameMode;
        set { _metadata.GameMode = value; _changes |= Changes.Params; }
    }

    public int MusicId
    {
        get => _metadata.MusicId;
        set { _metadata.MusicId = value; _changes |= Changes.Music; }
    }

    public Difficulty Difficulty
    {
        get => _metadata.Difficulty;
        set { _metadata.Difficulty = value; _changes |= Changes.Music; }
    }

    public GameSpeed Speed
    {
        get => _metadata.Speed;
        set { _metadata.Speed = value; _changes |= Changes.Music; }
    }

    public int MinLevelLimit
    {
        get => _metadata.MinLevelLimit;
        set { _metadata.MinLevelLimit = value; _changes |= Changes.Params; }
    }

    public int MaxLevelLimit
    {
        get => _metadata.MaxLevelLimit;
        set { _metadata.MaxLevelLimit = value; _changes |= Changes.Params; }
    }

    public int Arena
    {
        get => _metadata.Arena;
        set { _metadata.Arena = value; _changes |= Changes.Arena; }
    }

    public IList<int> Skills
    {
        get => _metadata.Skills;
        set { _metadata.Skills = value; _changes |= Changes.Skills; }
    }

    public int SkillsSeed
    {
        get => _metadata.SkillsSeed;
        set => _metadata.SkillsSeed = value;
    }

    public bool Premium => _metadata.Premium;

    public bool IsSelectingMusic { get; set; }
    public bool IsRelaySessionCreated { get; set; }

    public IScoreTracker ScoreTracker { get; private set; }

    public event EventHandler<RoomUserJoinedEventArgs>? UserJoined;
    public event EventHandler<RoomUserLeftEventArgs>? UserLeft;
    public event EventHandler<RoomUserLeftEventArgs>? UserDisconnected;
    public event EventHandler<RoomUserTeamChangedEventArgs>? UserTeamChanged;
    public event EventHandler<RoomUserMusicStateChangedEventArgs>? UserMusicStateChanged;
    public event EventHandler<RoomUserReadyStateChangedEventArgs>? UserReadyStateChanged;

    public event EventHandler<RoomParamsChangedEventArgs>? RoomParamsChanged;
    public event EventHandler<RoomMusicChangedEventArgs>? MusicChanged;
    public event EventHandler<RoomAlbumChangedEventArgs>? AlbumChanged;
    public event EventHandler<RoomArenaChangedEventArgs>? ArenaChanged;
    public event EventHandler<RoomStateChangedEventArgs>? StateChanged;
    public event EventHandler<RoomSlotChangedEventArgs>? SlotChanged;
    public event EventHandler<RoomSkillChangedEventArgs>? SkillChanged;

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

        int memberId = Attach(member);
        if (GameMode == GameMode.Live)
        {
            member.Role = memberId switch
            {
                0 => MemberRole.Champion,
                3 => MemberRole.Challenger,
                _ => MemberRole.Spectator
            };
        }

        UserJoined?.Invoke(this, new RoomUserJoinedEventArgs
        {
            MemberId = memberId,
            Member   = member
        });
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

        var removal = Detach(session);
        if (removal is not { } removed)
            return; // Might be kick left-over state

        int index = removed.MemberId;

        if (GameMode == GameMode.Live)
        {
            // DO NOT update master id after the re-arrangement

            var queueMembers = new List<MemberSlot>();
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i] is not MemberSlot m || _slots[i] is LockedSlot)
                    continue;

                if (i != 0 && index == 3 && _slots[index] is not MemberSlot)
                {
                    _slots[index] = m;
                    m.Role = MemberRole.Challenger;
                }
                else if (i == 3 && index == 0 && _slots[index] is not MemberSlot)
                {
                    _slots[index] = m;
                    _slots[i] = new VacantSlot();
                    m.Role = MemberRole.Champion;
                }
                else if (i >= 4)
                {
                    if (_slots[3] is not MemberSlot)
                    {
                        _slots[3] = m;
                        m.Role =  MemberRole.Challenger;
                    }
                    else if (_slots[0] is not MemberSlot)
                    {
                        _slots[0] = m;
                        m.Role =  MemberRole.Champion;
                    }
                    else
                    {
                        m.Role = MemberRole.Spectator;
                        queueMembers.Add(m);

                        _slots[i] = new VacantSlot();
                    }
                }
            }

            int placed = 0;
            for (int i = 4; i < _slots.Count && placed < queueMembers.Count; i++)
            {
                if (_slots[i] is LockedSlot)
                    continue;

                _slots[i] = queueMembers[placed++];
            }
        }

        ScoreTracker.Untrack(session);
        UserLeft?.Invoke(this, new RoomUserLeftEventArgs
        {
            MemberId           = index,
            Member             = removed.Member,
            RoomMasterMemberId = removed.MasterMemberId
        });

        if (!_slots.OfType<MemberSlot>().Any())
            _service.DeleteRoom(Channel, Id);

        SaveMetadataChanges();
    }

    public void SaveMetadataChanges()
    {
        if (_changes.HasFlag(Changes.Params))
        {
            RoomParamsChanged?.Invoke(this, new RoomParamsChangedEventArgs
            {
                Title         = _metadata.Title,
                Password      = _metadata.Password,
                MusicId       = _metadata.MusicId,
                KeyMode       = _metadata.KeyMode,
                GameMode      = _metadata.GameMode,
                MinLevelLimit = _metadata.MinLevelLimit,
                MaxLevelLimit = _metadata.MaxLevelLimit,
            });
        }

        if (_changes.HasFlag(Changes.Music))
        {
            MusicChanged?.Invoke(this, new RoomMusicChangedEventArgs
            {
                MusicId = _metadata.MusicId,
                Difficulty = _metadata.Difficulty,
                Speed = _metadata.Speed,
            });
        }

        if (_changes.HasFlag(Changes.Arena))
        {
            ArenaChanged?.Invoke(this, new RoomArenaChangedEventArgs
            {
                Arena = _metadata.Arena
            });
        }

        if (_changes.HasFlag(Changes.State))
        {
            StateChanged?.Invoke(this, new RoomStateChangedEventArgs
            {
                PreviousState = _previousState,
                CurrentState  = _metadata.State
            });
        }

        if (_changes.HasFlag(Changes.Skills))
        {
            SkillChanged?.Invoke(this, new RoomSkillChangedEventArgs
            {
                Skills = _metadata.Skills
            });
        }
        _previousState = _metadata.State;
        _changes       = Changes.None;
    }

    public void UpdateReadyState(Session session)
    {
        var change = ToggleReady(session);

        UserReadyStateChanged?.Invoke(this, new RoomUserReadyStateChangedEventArgs
        {
            MemberId = change.MemberId,
            Member   = change.Member,
            Ready    = change.Member.IsReady
        });
    }

    public void UpdateTeam(Session session, RoomTeam team)
    {
        var change = SetTeam(session, team);

        UserTeamChanged?.Invoke(this, new RoomUserTeamChangedEventArgs
        {
            MemberId = change.MemberId,
            Member   = change.Member,
            Team     = change.Member.Team
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
            MemberId     = index,
            Member       = member,
            PlayingState = member.PlayingState,
            MusicState   = state
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
            challenger.Role      = MemberRole.Champion;
            challenger.WinStreak = 0;
            _slots[0]            = challenger;

            challenger.IsMaster = true;
            challenger.IsReady  = false;
        }
        else
            champion.WinStreak++;

        var loser = !newChampion ? challenger : champion;
        _slots[3]  = new VacantSlot();

        bool promoted = false;
        for (int i = 4; i < _slots.Count; i++)
        {
            if (_slots[i] is not MemberSlot queued)
                continue;

            queued.Role = MemberRole.Challenger;
            _slots[3]   = queued;
            _slots[i]   = new VacantSlot();
            promoted    = true;
            break;
        }

        if (!promoted)
        {
            loser.Role = MemberRole.Challenger;
            _slots[3]  = loser;
            return;
        }

        loser.Role = MemberRole.Spectator;
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

        _changes |= Changes.State;
        SaveMetadataChanges();

        _ = ScheduleStartTimeout();
    }

    public void CompleteGame()
    {
        if (_metadata.State != RoomState.Playing)
            return;

        foreach (var member in _slots.OfType<MemberSlot>())
        {
            if (member.PlayingState !=  PlayingState.Waiting)
                member.IsReady = member.IsMaster;

            member.PlayingState = PlayingState.None;
            if (member.Role == MemberRole.Champion && GameMode == GameMode.Live)
                _metadata.Title = $"{member.WinStreak} Wins : {member.Actor.Nickname}";
                _changes |= Changes.Params;
        }

        _metadata.State = RoomState.Waiting;

        _changes |= Changes.State;

        SaveMetadataChanges();
    }

    protected override void RemoveSession(Session session) => ((IRoom)this).Remove(session);

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
