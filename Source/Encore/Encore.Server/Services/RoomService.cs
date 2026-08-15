using System.Collections.Concurrent;
using Encore.Entities;
using Encore.Server.Sessions;
using Encore.Sessions;
using Microsoft.Extensions.Logging;
using Mozart.Entities;

namespace Encore.Services;

public class RoomEventArgs : EventArgs
{
    public required IChannel Channel { get; init; }
    public required IRoom Room { get; init; }
}

public abstract class RoomService<TRoom>(ILogger logger) : Broadcastable
    where TRoom : class, IRoom, IRoomLifecycle
{
    private readonly ConcurrentDictionary<int, ConcurrentDictionary<int, TRoom>> _rooms = [];

    public event EventHandler<RoomEventArgs>? RoomCreated;
    public event EventHandler<RoomEventArgs>? RoomDeleted;

    public override IReadOnlyList<Session> Sessions =>
        _rooms.Values.SelectMany(e => e.Values.SelectMany(r => r.Sessions)).ToList();

    protected TRoom AddRoom(Session session, Func<IChannel, Func<int, TRoom>> createFactory)
    {
        if (session.Room != null)
            throw new ArgumentOutOfRangeException(nameof(session));

        if (session.Channel == null)
            throw new ArgumentOutOfRangeException(nameof(session));

        var channel = session.Channel;
        _rooms.TryAdd(channel.Id, []);

        var rooms = _rooms[channel.Id];
        if (rooms.Count >= channel.Capacity)
            throw new InvalidOperationException("Channel is full");

        var factory = createFactory(channel);
        for (int i = 0; i < channel.Capacity; i++)
        {
            var room = factory(i);
            if (!rooms.TryAdd(i, room))
                continue;

            session.Register(room);
            room.SessionDisconnected += OnRoomSessionDisconnected;

            RoomCreated?.Invoke(this, new RoomEventArgs
            {
                Channel = channel,
                Room    = room
            });
            return room;
        }

        throw new InvalidOperationException("Channel is full");
    }

    public TRoom DeleteRoom(IChannel channel, int id)
    {
        if (!_rooms.TryGetValue(channel.Id, out var rooms))
            throw new ArgumentOutOfRangeException(nameof(channel));

        if (!rooms.TryRemove(id, out var room))
            throw new ArgumentOutOfRangeException(nameof(id));

        RoomDeleted?.Invoke(this, new RoomEventArgs
        {
            Channel = channel,
            Room    = room
        });

        return room;
    }

    public TRoom GetRoom(IChannel channel, int id)
    {
        if (!_rooms.TryGetValue(channel.Id, out var rooms))
            throw new ArgumentOutOfRangeException(nameof(channel));

        if (!rooms.TryGetValue(id, out var room))
            throw new ArgumentOutOfRangeException(nameof(id));

        return room;
    }

    public IReadOnlyList<TRoom> GetRooms(IChannel channel)
    {
        _rooms.TryAdd(channel.Id, []);
        return _rooms[channel.Id].Values.ToList();
    }

    public override void Invalidate()
    {
        foreach (var session in Sessions)
        {
            if (session.Connected)
                continue;

            if (session.Channel != null)
                session.Exit(session.Channel);
            else if (session.Room != null)
                session.Exit(session.Room);
        }
    }

    private void OnRoomSessionDisconnected(object? sender, SessionEventArgs<TcpSession> e)
    {
        logger.LogWarning("Session [{User}] removed from the room due to connection lost",
            e.Session.Socket.RemoteEndPoint);
    }
}
