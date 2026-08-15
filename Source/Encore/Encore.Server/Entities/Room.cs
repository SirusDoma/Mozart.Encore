using Encore.Metadata;
using Encore.Metadata.Room;
using Encore.Server.Sessions;
using Encore.Sessions;
using Mozart.Sessions;

namespace Encore.Entities;

public abstract class Room : Broadcastable, IRoomLifecycle
{
    public const byte MaxCapacity = 8;

    protected readonly List<ISlot> _slots;

    protected Room(Session master)
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
            new VacantSlot()
        ];

        Channel = master.Channel!;
    }

    public interface ISlot;

    public class VacantSlot : ISlot;

    public class LockedSlot : ISlot;

    public class MemberSlot : ISlot
    {
        public required Session Session { get; init; }

        public required RoomTeam Team { get; set; }

        public MemberRole Role { get; set; }

        public int WinStreak { get; set; }

        public bool IsMaster { get; set; }

        public bool IsReady { get; set; }

        public MusicState MusicState { get; set; } = MusicState.Ready;

        public PlayingState PlayingState { get; set; } = PlayingState.None;

        public Actor Actor => Session.GetAuthorizedToken<Actor>();
    }

    protected readonly record struct MemberChange(int MemberId, MemberSlot Member);

    protected readonly record struct MemberRemoval(int MemberId, MemberSlot Member, int MasterMemberId);

    protected readonly record struct SlotChange(
        ISlot PreviousSlot,
        ISlot CurrentSlot,
        RoomSlotActionType ActionType);

    public IChannel Channel { get; }

    public int Capacity => Slots.Count(s => s is not LockedSlot);

    public int UserCount => Slots.Count(s => s is MemberSlot);

    public int PlayingUserCount => Slots.Count(s => s is MemberSlot { PlayingState: PlayingState.Playing });

    public Session Master => _slots.OfType<MemberSlot>().Single(s => s.IsMaster).Session;

    public IReadOnlyList<ISlot> Slots => _slots;

    public override IReadOnlyList<Session> Sessions
        => _slots.OfType<MemberSlot>().Select(m => m.Session).ToList();

    public event EventHandler<SessionEventArgs<TcpSession>>? SessionDisconnected;

    public bool IsMember(Session session)
    {
        return _slots.OfType<MemberSlot>().Any(m => m.Session == session);
    }

    public bool IsMember(Actor actor)
    {
        return _slots.OfType<MemberSlot>().Any(m => m.Actor == actor);
    }

    protected int Attach(MemberSlot member)
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            if (_slots[i] is not VacantSlot)
                continue;

            _slots[i] = member;
            return i;
        }

        throw new InvalidOperationException("Room is full");
    }

    protected MemberRemoval? Detach(Session session)
    {
        int index = _slots.FindIndex(s => s is MemberSlot m && m.Session == session);
        if (index < 0)
            return null;

        var member = (MemberSlot)_slots[index];
        int masterId = _slots.FindIndex(s => s is MemberSlot { IsMaster: true });

        _slots[index] = new VacantSlot();
        if (member.IsMaster)
        {
            masterId = _slots.FindIndex(s => s is MemberSlot m && m.Session != session);
            if (masterId >= 0)
            {
                var master = (MemberSlot)_slots[masterId];
                master.IsMaster = true;
                master.IsReady  = true;
            }
        }

        return new MemberRemoval(index, member, masterId);
    }

    protected MemberChange ToggleReady(Session session)
    {
        var member = GetMember(session);
        member.Member.IsReady = !member.Member.IsReady;
        return member;
    }

    protected MemberChange SetTeam(Session session, RoomTeam team)
    {
        var member = GetMember(session);
        member.Member.Team = team;
        return member;
    }

    protected SlotChange ToggleSlot(int slotId)
    {
        if (slotId is < 0 or >= MaxCapacity)
            throw new ArgumentOutOfRangeException(nameof(slotId));

        var previous = _slots[slotId];
        var action = RoomSlotActionType.PlayerKicked;

        _slots[slotId] = previous switch
        {
            MemberSlot => new VacantSlot(),
            LockedSlot => new VacantSlot(),
            VacantSlot => new LockedSlot(),
            _ => throw new InvalidOperationException("Unknown room slot type")
        };

        if (previous is LockedSlot)
            action = RoomSlotActionType.SlotUnlocked;
        else if (previous is VacantSlot)
            action = RoomSlotActionType.SlotLocked;

        return new SlotChange(previous, _slots[slotId], action);
    }

    public void Disconnect(Session session)
    {
        RemoveSession(session);
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

    protected abstract void RemoveSession(Session session);

    private MemberChange GetMember(Session session)
    {
        int index = _slots.FindIndex(s => s is MemberSlot m && m.Session == session);
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(session));

        return new MemberChange(index, (MemberSlot)_slots[index]);
    }
}
