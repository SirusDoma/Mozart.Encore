using Mozart.Options;
using Mozart.Services;
using Mozart.Sessions;

namespace Mozart.Entities;

public interface IChannel : IBroadcastable
{
    int Id         { get; }
    int Capacity   { get; }
    float GemRates { get; }
    float ExpRates { get; }

    string ItemDataPath { get; }

    ValueTask Register(Session session, CancellationToken cancellationToken);

    ValueTask Remove(Session session, CancellationToken cancellationToken);
}

public class Channel() : Broadcastable, IChannel
{
    private readonly List<Session> _sessions = [];

    public Channel(ChannelOptions options)
        : this()
    {
        Id       = options.Id;
        Capacity = options.Capacity;
        GemRates = options.Gem;
        ExpRates = options.Exp;

        ItemDataPath = options.ItemData;
    }

    public override IReadOnlyList<Session> Sessions => _sessions.ToList();

    public event EventHandler? SessionDisconnected;

    public int Id         { get; init; }
    public int Capacity   { get; init; }
    public float GemRates { get; init; }
    public float ExpRates { get; init; }

    public string ItemDataPath { get; init; } = string.Empty;

    public int UserCount => _sessions.Count;

    ValueTask IChannel.Register(Session session, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(session.Authorized, true, nameof(session));

        if (session.Channel == null)
            return session.Register(this, cancellationToken);

        if (_sessions.Count > Capacity)
            throw new InvalidOperationException("Channel is full");

        session.Disconnected += OnSessionDisconnected;
        _sessions.Add(session);

        return ValueTask.CompletedTask;
    }

    ValueTask IChannel.Remove(Session session, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(session.Authorized, true, nameof(session));

        if (session.Channel != null)
        {
            if (session.Channel != this)
                throw new ArgumentOutOfRangeException(nameof(session));

            return session.Exit(this, cancellationToken);
        }

        _sessions.Remove(session);
        session.Disconnected -= OnSessionDisconnected;

        return ValueTask.CompletedTask;
    }

    private async void OnSessionDisconnected(object? sender, EventArgs args)
    {
        try
        {
            var session = (Session)sender!;
            if (session.Room != null)
            {
                var room = (Room)session.Room;
                await room.Disconnect(session, CancellationToken.None);
            }

            IChannel channel = this;
            await channel.Remove(session, CancellationToken.None);

            SessionDisconnected?.Invoke(sender, args);
        }
        catch (Exception ex)
        {
            SessionDisconnected?.Invoke(sender, new Encore.Sessions.SessionErrorEventArgs()
            {
                Session   = ((Session?)sender)!,
                Exception = ex
            });
        }
    }

    protected override IEnumerable<Session> GetSessionsByContext<TContext>(TContext ctx)
    {
        if (ctx is WhisperMessageContext whisper)
            return _sessions.Where(s => whisper.Recipients.Contains(s.GetAuthorizedToken<Actor>().Nickname));

        return [];
    }
}