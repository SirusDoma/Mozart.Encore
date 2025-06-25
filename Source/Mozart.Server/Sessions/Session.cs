using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Options;

using Encore;
using Encore.Messaging;
using Encore.Server;

using Mozart.Entities;
using Mozart.Options;

namespace Mozart.Sessions;

public class Session : Encore.Sessions.Session
{
    private readonly IMessageCodec _codec;

    public Actor Actor => GetAuthorizedToken<Actor>();

    public IChannel? Channel { get; private set; }

    public IRoom? Room { get; private set; }

    public Session(TcpClient client, IOptions<TcpOptions> options, IMessageFramerFactory framer,
        ICommandDispatcher dispatcher, IMessageCodec codec) : base(client, options.Value, framer, dispatcher)
    {
        _codec = codec;
    }

    public async Task WriteMessage<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : class, IMessage
    {
        _codec.Register<TMessage>();
        await WriteFrame(_codec.Encode(message), cancellationToken).ConfigureAwait(false);
    }

    public void Register(IChannel channel)
    {
        if (Channel != null)
            Exit(Channel!);

        Channel = channel;
        Channel.Register(this);
    }

    public void Exit(IChannel channel)
    {
        if (Channel == null)
            return;

        if (channel != Channel)
            throw new ArgumentOutOfRangeException(nameof(channel));

        if (Room != null)
            Exit(Room);

        Channel = null;
        channel.Remove(this);
    }

    public void Register(IRoom room)
    {
        if (Room != null)
            Exit(Room);

        Room = room;
        Room.Register(this);
    }

    public void Exit(IRoom room)
    {
        if (Room == null)
            return;

        if (room != Room)
            throw new ArgumentOutOfRangeException(nameof(room));

        Room = null;
        room.Remove(this);
    }

    public void Kick()
    {
        Room = null;
    }
}