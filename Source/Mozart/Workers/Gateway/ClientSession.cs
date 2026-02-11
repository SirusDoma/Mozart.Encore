using System.Net.Sockets;
using Encore.Messaging;
using Encore.Server;
using Microsoft.Extensions.Options;
using Mozart.Options;
using Mozart.Sessions;

namespace Mozart.Workers.Gateway;

public class ClientSession : Session
{
    private ChannelSession? _channelSession;

    public int? ChannelId => _channelSession?.ChannelId;

    public ClientSession(TcpClient client, IOptions<TcpOptions> options, IMessageFramerFactory framer,
        ICommandDispatcher dispatcher, IMessageCodec codec) : base(client, options, framer, dispatcher, codec)
    {
    }

    public string Id { get; private set; } = string.Empty;

    public bool HasChannelSession => _channelSession != null;

    public void Assign(string id)
    {
        Id = id;
    }

    public void Register(ChannelSession session)
    {
        _channelSession = session;
    }

    public override void Terminate()
    {
        base.Terminate();
        _channelSession?.Terminate(this);
    }
}
