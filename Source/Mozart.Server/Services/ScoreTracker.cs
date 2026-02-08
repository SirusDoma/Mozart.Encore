using Mozart.Entities;
using Mozart.Metadata;
using Mozart.Sessions;

namespace Mozart.Services;

public interface IScoreTracker
{
    bool Completed { get; }
    int Count { get; }

    void UpdateLife(Session session, int life);
    void UpdateJamCombo(Session session, int combo);

    bool IsTracked(Session session);
    void Track(Session session);
    void Untrack(Session session);

    void SubmitScore(Session session, int cool, int good, int bad, int miss,
        int maxCombo, int maxJamCombo, uint score, int life);
}

public class ScoreUpdateEventArgs : EventArgs
{
    public required int MemberId    { get; init; }
    public required Session Session { get; init; }
    public required int Value       { get; init; }
}

public class ScoreTrackEventArgs : EventArgs
{
    public required int MemberId    { get; init; }
    public required Session Session { get; init; }
}

public class ScoreSubmitEventArgs : EventArgs
{
    public required int MemberId { get; init; }
}

public class ScoreTrackedEventArgs : EventArgs
{
    public required IRoom Room { get; init; }
    public int MusicId { get; init; }
    public Difficulty Difficulty { get; init; }
    public required IReadOnlyList<ScoreTracker.UserScore> States { get; init; }
    public required GameMode Mode { get; init; }
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

    private readonly List<UserScore> _states = [];
    private readonly Dictionary<int, List<UserScore>> _scores = [];

    public EventHandler<ScoreTrackEventArgs>? UserTracked;
    public EventHandler<ScoreTrackEventArgs>? UserUntracked;

    public EventHandler<ScoreUpdateEventArgs>?  UserLifeUpdated;
    public EventHandler<ScoreUpdateEventArgs>?  UserJamIncreased;
    public EventHandler<ScoreSubmitEventArgs>?  UserScoreSubmitted;
    public EventHandler<ScoreTrackedEventArgs>? ScoreCompleted;

    public IRoom Room { get; }

    public int Count => _states.Count;

    public bool Completed => _states.Count == 0 || _states.All(s => s.Completed);

    public ScoreTracker(IRoom room)
    {
        Room = room;
    }

    public void UpdateLife(Session session, int life)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(life, 1000, nameof(life));
        ArgumentOutOfRangeException.ThrowIfNegative(life, nameof(life));

        var state = _states.SingleOrDefault(s => s.Session == session);
        if (state == null)
            return; // might be left-over after leaving during gameplay

        state.Life = life;
        if (state is { Completed: false, Life: 0 })
        {
            state.Clear     = false;
            state.Completed = true;
        }

        UserLifeUpdated?.Invoke(this, new ScoreUpdateEventArgs
        {
            MemberId = state.MemberId,
            Session  = state.Session,
            Value    = life
        });
    }

    public void UpdateJamCombo(Session session, int jamCombo)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(jamCombo, nameof(jamCombo));

        var state = _states.SingleOrDefault(s => s.Session == session);
        if (state == null)
            return; // might be left-over after leaving during gameplay

        state.JamCombo = jamCombo;
        UserJamIncreased?.Invoke(this, new ScoreUpdateEventArgs
        {
            MemberId = state.MemberId,
            Session  = state.Session,
            Value    = jamCombo
        });
    }

    public bool IsTracked(Session session)
    {
        return _states.Any(s => s.Session == session);
    }

    public void Track(Session session)
    {
        if (_states.Count >= Room.Capacity)
            throw new InvalidOperationException("All members are already tracked");

        for (int i = 0; i < Room.Slots.Count; i++)
        {
            if (Room.Slots[i] is not Room.MemberSlot member)
                continue;

            if (session != member.Session)
                continue;

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

            UserTracked?.Invoke(this, new ScoreTrackEventArgs
            {
                MemberId = i,
                Session  = member.Session
            });

            if (_states.Count == Room.Slots.OfType<Room.MemberSlot>().Count())
            {
                for (int j = 0; j < Entities.Room.MaxCapacity; j++)
                {
                    if (Room.Slots[j] is Entities.Room.VacantSlot or Entities.Room.LockedSlot)
                    {
                        UserTracked?.Invoke(this, new ScoreTrackEventArgs
                        {
                            MemberId = j,
                            Session = null!
                        });
                    }
                }
            }

            return;
        }

        throw new ArgumentOutOfRangeException(nameof(session), "Session is not recognized");
    }

    public void Untrack(Session session)
    {
        var state = _states.SingleOrDefault(s => s.Session == session);
        if (state == null)
            return;

        state.Session.Disconnected -= OnSessionDisconnected;
        _states.Remove(state);

        UserUntracked?.Invoke(this, new ScoreTrackEventArgs
        {
            MemberId = state.MemberId,
            Session  = session
        });

        if (Room.State == RoomState.Playing)
        {
            // Client send exit room, but just to be safe - let's remove the member here
            // Probably need to revise in the future network version
            if (Room.Metadata.Mode != GameMode.Single)
                state.Session.Exit(Room);
            else if (Completed)
                Room.CompleteGame();
        }
    }

    public void SubmitScore(Session session, int cool, int good, int bad, int miss, int maxCombo,
        int maxJamCombo, uint score, int life)
    {
        var state = _states.SingleOrDefault(s => s.Session == session);
        if (state == null)
            throw new ArgumentOutOfRangeException(nameof(session)); // request forged?

        state.Cool        = cool;
        state.Good        = good;
        state.Bad         = bad;
        state.Miss        = miss;
        state.MaxCombo    = maxCombo;
        state.MaxJamCombo = maxJamCombo;
        state.Score       = score;
        state.Life        = life;
        state.Clear       = state.Life > 0;
        state.Completed   = true;

        state.Session.Disconnected -= OnSessionDisconnected;
        var completedStates = _states.Where(s => s.Completed).ToList();

        UserScoreSubmitted?.Invoke(this, new ScoreSubmitEventArgs
        {
            MemberId = state.MemberId
        });

        if (Completed)
        {
            // By default, the game finished after a song is ended
            bool finalized = true;

            // However, album mode plays several songs in succession
            // TODO: Create a separate score tracker class for album mode
            if (Room.Mode == GameMode.Jam)
            {
                var album   = Room.Channel.GetAlbumList()[Room.MusicId];
                var current = album.Entries[_scores.TryGetValue(state.MemberId, out var last) ? last.Count : 0];

                // Trigger score completion for current song
                ScoreCompleted?.Invoke(this, new ScoreTrackedEventArgs
                {
                    Room       = Room,
                    MusicId    = current.Id,
                    Difficulty = current.Difficulty,
                    States     = completedStates,
                    Mode       = GameMode.Versus
                });

                // In album mode, scores submitted each music transition.
                // Therefore, capture the score and clear the tracker states once all scores are submitted.
                foreach (var completedState in completedStates)
                {
                    int memberId = completedState.MemberId;
                    if (!_scores.ContainsKey(memberId))
                        _scores[memberId] = [];

                    _scores[memberId].Add(completedState);
                }

                // Determine whether the game is completed (all songs are completed)
                finalized = _states.All(s => s.Life == 0) || _scores.Max(c => c.Value.Count) >= album.Entries.Count;

                // Re-build the states, either for finalized states or states for the next song
                // The client will issue `ConfirmMusicLoaded` request during the song transition
                _states.Clear();

                if (finalized)
                {
                    foreach ((int memberId, var finalState) in _scores)
                    {
                        _states.Add(new UserScore
                        {
                            Session     = finalState.Last().Session,
                            MemberId    = memberId,
                            Cool        = finalState.Sum(s => s.Cool),
                            Good        = finalState.Sum(s => s.Good),
                            Bad         = finalState.Sum(s => s.Bad),
                            Miss        = finalState.Sum(s => s.Miss),
                            MaxCombo    = finalState.Max(s => s.MaxCombo),
                            MaxJamCombo = finalState.Max(s => s.MaxJamCombo),
                            Score       = (uint)finalState.Sum(s => s.Score),
                            Life        = finalState.Last().Life,
                            Clear       = finalState.Last().Life > 0,
                            Completed   = true,
                        });
                    }

                    // Trigger album completion event
                    ScoreCompleted?.Invoke(this, new ScoreTrackedEventArgs
                    {
                        Room    = Room,
                        States  = completedStates,
                        Mode    = GameMode.Jam
                    });
                }
            }
            else
            {
                // Trigger normal score completion
                ScoreCompleted?.Invoke(this, new ScoreTrackedEventArgs
                {
                    Room       = Room,
                    MusicId    = Room.MusicId,
                    Difficulty = Room.Difficulty,
                    States     = completedStates,
                    Mode       = Room.Mode
                });

            }

            // The room marked as `Waiting` after the first `ExitPlaying` received in the official semantic.
            // However, performing early clean-up increase robustness. e.g, less room stuck due to network issue
            if (finalized)
            {
                foreach (var member in Room.Slots.OfType<Room.MemberSlot>())
                    member.IsReady = member.IsMaster;

                Room.CompleteGame();
            }
        }
    }

    public void OnSessionDisconnected(object? sender, EventArgs e)
    {
        Untrack((Session)sender!);
    }
}
