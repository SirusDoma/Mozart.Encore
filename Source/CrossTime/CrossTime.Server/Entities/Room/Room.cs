using Encore.Data.Entities;
using Encore.Metadata;
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
        ScoreTracker = new ScoreTracker(this);
    }

    [Flags]
    private enum Changes
    {
        None  = 0,
        Music = 1 << 0,
        Arena = 1 << 1,
        State = 1 << 2,
        Skills = 1 << 3,
        Params = 1 << 4,
        Team  = 1 << 5
    }

    public int Id => _metadata.Id;

    public RoomState State => _metadata.State;

    public int MinLevelLimit => _metadata.MinLevelLimit;

    public int MaxLevelLimit => _metadata.MaxLevelLimit;

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

    public int MusicId
    {
        get => _metadata.MusicId;
        set { _metadata.MusicId = value; _changes |= Changes.Music; }
    }


    public int MissionLevel
    {
        get => _metadata.MissionLevel;
        set { _metadata.MissionLevel = value; _changes |= Changes.Music; }
    }

    public GameMode Mode
    {
        get => _metadata.Mode;
        set { _metadata.Mode = value; _changes |= Changes.Params; }
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

    public bool TeamEnabled
    {
        get => _metadata.TeamEnabled;
        set { _metadata.TeamEnabled = value; _changes |= Changes.Team; }
    }

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

        int memberId = Attach(member);
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

        if (State == RoomState.Playing)
            ScoreTracker.Untrack(session);

        UserLeft?.Invoke(this, new RoomUserLeftEventArgs
        {
            MemberId           = removed.MemberId,
            Member             = removed.Member,
            RoomMasterMemberId = removed.MasterMemberId
        });

        if (!_slots.OfType<MemberSlot>().Any())
            _service.DeleteRoom(Channel, Id);
    }

    public void SaveMetadataChanges()
    {
        if (_changes.HasFlag(Changes.Music))
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

        if (_changes.HasFlag(Changes.Params))
        {
            ModeChanged?.Invoke(this, new RoomModeChangedEventArgs
            {
                Title       = _metadata.Title,
                Mode        = _metadata.Mode,
                Password    = _metadata.Password,
                TeamEnabled = _metadata.TeamEnabled
            });
        }

        if (_changes.HasFlag(Changes.Team))
        {
            TeamToggleChanged?.Invoke(this, new RoomTeamToggleChangedEventArgs
            {
                Enabled = _metadata.TeamEnabled
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

    public void UpdateMusicState(Session session, int memberId)
    {
        if (memberId is < 0 or >= MaxCapacity)
            throw new ArgumentOutOfRangeException(nameof(memberId));

        if (_slots[memberId] is not MemberSlot member)
            throw new ArgumentOutOfRangeException(nameof(memberId));

        var state = MusicState.Ready;

        if (!_options.FreeMission)
        {
            if (Channel.GetMusicList().TryGetValue(MusicId, out _)
                && member.Actor.FreePass.Type == FreePassType.None
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

        var change = ToggleSlot(slotId);

        SlotChanged?.Invoke(this, new RoomSlotChangedEventArgs
        {
            SlotId       = slotId,
            PreviousSlot = change.PreviousSlot,
            CurrentSlot  = change.CurrentSlot,
            ActionType   = change.ActionType,
            Capacity     = Capacity,
            UserCount    = UserCount
        });

        if (change.PreviousSlot is MemberSlot member)
            member.Session.Exit(this);
    }

    public void StartGame()
    {
        ScoreTracker = new ScoreTracker(this);

        _metadata.State = RoomState.Playing;

        _changes |= Changes.State;
        SaveMetadataChanges();

        _ = ScheduleStartTimeout();
    }

    public void CompleteGame()
    {
        if (!ScoreTracker.Completed || _metadata.State != RoomState.Playing)
            return;

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
