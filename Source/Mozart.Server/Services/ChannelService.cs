using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Mozart.Entities;
using Mozart.Options;
using Mozart.Sessions;

namespace Mozart.Services;

public interface IChannelService : IBroadcastable
{
    IReadOnlyList<IChannel> GetChannels();
    IReadOnlyList<Actor> GetUserList(int id);
    void CreateChannel(ChannelOptions channelOptions);
    IChannel GetChannel(int id);
    void DeleteChannel(int id);
}

public class ChannelService : Broadcastable, IChannelService
{
    private readonly ConcurrentDictionary<int, IChannel> _channels;

    private readonly ILogger<ChannelService> _logger;

    public ChannelService(IOptions<ServerOptions> options, ILogger<ChannelService> logger,
        IOptions<GatewayOptions> gatewayOptions)
    {
        _logger = logger;

        if (options.Value.Mode != DeploymentMode.Gateway)
        {
            _channels = new ConcurrentDictionary<int, IChannel>(gatewayOptions.Value.Channels.ToDictionary(
                c => c.Id,
                IChannel (c) =>
                {
                    var channel = new Channel(c);
                    channel.SessionDisconnected += OnChannelSessionDisconnected;

                    return channel;
                }
            ));
        }
        else
        {
            _channels = [];
        }
    }

    public override IReadOnlyList<Session> Sessions
        => _channels.Values.SelectMany(c => c.Sessions).ToList();

    public IReadOnlyList<IChannel> GetChannels()
        => _channels.Values.ToList();

    public IReadOnlyList<Actor> GetUserList(int id)
        => _channels[id].Sessions.Select(e => e.GetAuthorizedToken<Actor>()).ToList();

    public IChannel GetChannel(int id)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(id, nameof(id));
        if (!_channels.TryGetValue(id, out var channel))
            throw new ArgumentOutOfRangeException(nameof(id));

        return channel;
    }

    public void CreateChannel(ChannelOptions channelOptions)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(channelOptions.Id, nameof(channelOptions));

        var channel = new Channel(channelOptions);
        if (!_channels.TryAdd(channelOptions.Id, channel))
            throw new ArgumentOutOfRangeException(nameof(channelOptions));

        channel.SessionDisconnected += OnChannelSessionDisconnected;
    }

    public void DeleteChannel(int id)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(id, nameof(id));

        _channels.Remove(id, out _);
    }

    public override void Invalidate()
    {
        foreach (var session in Sessions)
        {
            if (session.Connected)
                continue;

            if (session.Channel != null)
                session.Exit(session.Channel);
        }
    }

    private void OnChannelSessionDisconnected(object? sender, EventArgs args)
    {
        if (args is Encore.Sessions.SessionErrorEventArgs argsEx)
        {
            _logger.LogError(
                argsEx.Exception,
                "Session [{User}] removed from the channel due to connection lost with exception",
                ((Session?)sender)?.Socket.RemoteEndPoint
            );
        }
        else
        {
            _logger.LogWarning("Session [{User}] removed from the channel due to connection lost",
                ((Session?)sender)?.Socket.RemoteEndPoint);
        }
    }
}