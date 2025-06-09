using Mozart.Entities;
using Mozart.Messages;
using Mozart.Messages.Events;
using Mozart.Messages.Requests;
using Mozart.Sessions;

namespace Mozart.Services;

public interface IScoreTracker
{
    bool Completed { get; }
    int Count { get; }

    void Reset();

    ValueTask UpdateLife(Session session, int life, CancellationToken cancellationToken);
    ValueTask UpdateJamCombo(Session session, int combo, CancellationToken cancellationToken);

    ValueTask Untrack(Session session, CancellationToken cancellationToken);
    Task<IReadOnlyList<ScoreTracker.UserScore>> SubmitScore(Session session, ScoreSubmissionRequest request,
        CancellationToken cancellationToken);
}

public class ScoreTracker : IScoreTracker
{
    public class UserScore
    {
        public required Session Session { get; init; }
        public required int MemberId    { get; init; }

        public int Life        { get; set; } = 1000;
        public int JamCombo    { get; set; } = 0;

        public int Cool        { get; set; } = 0;
        public int Good        { get; set; } = 0;
        public int Bad         { get; set; } = 0;
        public int Miss        { get; set; } = 0;

        public int MaxCombo    { get; set; } = 0;
        public int MaxJamCombo { get; set; } = 0;
        public uint Score      { get; set; } = 0;

        public bool Clear      { get; set; } = false;
        public bool Completed  { get; set; } = false;
    }

    private readonly IRoom _room;
    private readonly List<UserScore> _states = [];

    public int Count => _states.Count;

    public bool Completed => _states.Count == 0 || _states.All(s => s.Completed);

    public ScoreTracker(IRoom room)
    {
        _room = room;
    }

    public void Reset()
    {
        _states.Clear();
        for (int i = 0; i < _room.Slots.Count; i++)
        {
            if (_room.Slots[i] is not Room.MemberSlot member)
                continue;

            member.Session.Disconnected -= OnSessionDisconnected;
            member.Session.Disconnected += OnSessionDisconnected;

            _states.Add(new UserScore
            {
                Session     = member.Session,
                MemberId    = i,
                Life        = 1000,
                JamCombo    = 0,
                Cool        = 0,
                Good        = 0,
                Bad         = 0,
                Miss        = 0,
                MaxCombo    = 0,
                MaxJamCombo = 0,
                Score       = 0,
                Clear       = false,
                Completed   = false
            });
        }
    }

    public async ValueTask UpdateLife(Session session, int life, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(life, 1000, nameof(life));
        ArgumentOutOfRangeException.ThrowIfNegative(life, nameof(life));

        var state = _states.SingleOrDefault(s => s.Session == session);
        if (state == null)
            throw new ArgumentOutOfRangeException(nameof(session)); // request forged?

        state.Life = life;
        if (state is { Completed: false, Life: 0 })
        {
            state.Clear     = false;
            state.Completed = true;
        }

        await _room.Broadcast(new GameStatsUpdateEventData
        {
            MemberId = (byte)state.MemberId,
            Type     = GameUpdateStatsType.Life,
            Value    = (ushort)life
        }, cancellationToken);
    }

    public async ValueTask UpdateJamCombo(Session session, int jamCombo, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(jamCombo, nameof(jamCombo));

        var state = _states.SingleOrDefault(s => s.Session == session);
        if (state == null)
            throw new ArgumentOutOfRangeException(nameof(session)); // request forged?

        state.JamCombo = jamCombo;
        await _room.Broadcast(new GameStatsUpdateEventData
        {
            MemberId = (byte)state.MemberId,
            Type     = GameUpdateStatsType.Jam,
            Value    = (ushort)jamCombo
        }, cancellationToken);
    }

    public async ValueTask Untrack(Session session, CancellationToken cancellationToken)
    {
        var state = _states.SingleOrDefault(s => s.Session == session);
        if (state == null)
            return;

        state.Session.Disconnected -= OnSessionDisconnected;
        _states.Remove(state);

        await _room.Broadcast(new UserLeaveGameEventData
        {
            MemberId = (byte)state.MemberId,
            Level    = session.Actor.Level,
        }, cancellationToken);
    }

    public async Task<IReadOnlyList<UserScore>> SubmitScore(Session session, ScoreSubmissionRequest request,
        CancellationToken cancellationToken)
    {
        var state = _states.SingleOrDefault(s => s.Session == session);
        if (state == null)
            throw new ArgumentOutOfRangeException(nameof(session)); // request forged?

        state.Cool        = request.Cool;
        state.Good        = request.Good;
        state.Bad         = request.Bad;
        state.Miss        = request.Miss;
        state.MaxCombo    = request.MaxCombo;
        state.MaxJamCombo = request.MaxJamCombo;
        state.Score       = request.Score;
        state.Life        = request.Life;
        state.Clear       = state.Life > 0;
        state.Completed   = true;

        await _room.Broadcast(new ScoreSubmissionEventData
        {
            MemberId = (byte)state.MemberId
        }, cancellationToken);

        return _states.Where(s => s.Completed).ToList();
    }

    public async void OnSessionDisconnected(object? sender, EventArgs e)
    {
        try
        {
            await Untrack((Session)sender!, CancellationToken.None);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}
