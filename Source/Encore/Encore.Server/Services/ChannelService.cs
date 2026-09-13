using System.Collections.Concurrent;
using System.Net;
using Encore.Entities;
using Encore.Metadata;
using Encore.Options;
using Encore.Server.Sessions;
using Encore.Sessions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mozart.Options;
using Mozart.Sessions;

namespace Encore.Services;

public interface IChannelService : IBroadcastable
{
    IReadOnlyList<IChannel> GetChannels();
    IReadOnlyList<Actor> GetUserList(int id);
    IChannel CreateChannel(IPEndPoint endPoint, int id, int capacity);
    bool DeleteChannel(IChannel channel);
    IChannel? FindChannel(int id);
    IChannel GetChannel(int id);
}

public class ChannelService : Broadcastable, IChannelService
{
    private readonly ConcurrentDictionary<int, IChannel> _channels;

    private readonly IMetadataResolver _metadataResolver;

    private readonly ILogger<ChannelService> _logger;

    public ChannelService(
        IMetadataResolver metadataResolver,
        IOptions<ServerOptions> options,
        IOptions<GatewayOptions> gatewayOptions,
        ILogger<ChannelService> logger
    )
    {
        _metadataResolver = metadataResolver;
        _logger = logger;

        if (options.Value.Mode != DeploymentMode.Gateway)
        {
            _channels = new ConcurrentDictionary<int, IChannel>(gatewayOptions.Value.Channels.ToDictionary(
                c => c.Id,
                IChannel (c) =>
                {
                    var channel = new Channel(_metadataResolver, c);
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

    public IChannel? FindChannel(int id)
        => _channels.GetValueOrDefault(id);

    public IChannel GetChannel(int id)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(id);
        if (!_channels.TryGetValue(id, out var channel))
            throw new ArgumentOutOfRangeException(nameof(id));

        return channel;
    }

    public IChannel CreateChannel(IPEndPoint endPoint, int id, int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(id);
        ArgumentNullException.ThrowIfNull(endPoint);

        var channel = new Channel(_metadataResolver, new ChannelOptions { Id = id, Capacity = capacity })
        {
            EndPoint = endPoint
        };
        channel.SessionDisconnected += OnChannelSessionDisconnected;

        if (!_channels.TryAdd(id, channel))
            throw new InvalidOperationException($"Channel [{id:00}] is already registered");

        return channel;
    }

    public bool DeleteChannel(IChannel channel)
        => _channels.TryRemove(KeyValuePair.Create(channel.Id, channel));

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
        if (args is SessionErrorEventArgs argsEx)
        {
            _logger.LogError(
                argsEx.Exception,
                "Session [{User}] removed from the channel due to connection lost with exception",
                ((Session?)sender)?.Socket?.RemoteEndPoint
            );
        }
        else
        {
            _logger.LogWarning("Session [{User}] removed from the channel due to connection lost",
                ((Session?)sender)?.Socket?.RemoteEndPoint);
        }
    }
}
