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

    void Register(Session session);

    void Remove(Session session);
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

    void IChannel.Register(Session session)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(session.Authorized, true, nameof(session));

        if (session.Channel == null)
            session.Register(this);

        if (_sessions.Count > Capacity)
            throw new InvalidOperationException("Channel is full");

        session.Disconnected += OnSessionDisconnected;
        _sessions.Add(session);
    }

    void IChannel.Remove(Session session)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(session.Authorized, true, nameof(session));

        if (session.Channel != null)
        {
            if (session.Channel != this)
                throw new ArgumentOutOfRangeException(nameof(session));

            session.Exit(this);
        }

        _sessions.Remove(session);
        session.Disconnected -= OnSessionDisconnected;
    }

    private void OnSessionDisconnected(object? sender, EventArgs args)
    {
        try
        {
            var session = (Session)sender!;
            if (session.Room != null)
            {
                var room = (Room)session.Room;
                room.Disconnect(session);
            }

            IChannel channel = this;
            channel.Remove(session);

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
}