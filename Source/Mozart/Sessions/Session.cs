using System.Net.Sockets;
using Microsoft.Extensions.Options;

using Encore.Messaging;
using Encore.Server;
using Mozart.Entities;


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
        await Framer.WriteFrame(_codec.Encode(message), cancellationToken);
    }

    public async ValueTask Register(IChannel channel, CancellationToken cancellationToken)
    {
        if (Channel != null)
            await Exit(Channel!, cancellationToken);

        Channel = channel;
        await Channel.Register(this, cancellationToken);
    }

    public ValueTask Exit(IChannel channel, CancellationToken cancellationToken)
    {
        if (channel != Channel)
            throw new ArgumentOutOfRangeException(nameof(channel));

        Channel = null;
        return channel.Remove(this, cancellationToken);
    }

    public async ValueTask Register(IRoom room, CancellationToken cancellationToken)
    {
        if (Room != null)
            await Exit(Room, cancellationToken);

        Room = room;
        await Room.Register(this, cancellationToken);
    }

    public ValueTask Exit(IRoom room, CancellationToken cancellationToken)
    {
        if (room != Room)
            throw new ArgumentOutOfRangeException(nameof(room));

        Room = null;
        return room.Remove(this, cancellationToken);
    }

    public void Kick()
    {
        Room = null;
    }
}