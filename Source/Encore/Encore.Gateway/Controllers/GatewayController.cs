using System.Net;
using System.Net.Sockets;
using Encore.Entities;
using Encore.Messages.Requests;
using Encore.Messages.Responses;
using Encore.Server;
using Encore.Server.Sessions;
using Encore.Services;
using Encore.Workers.Gateway;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mozart.Options;

namespace Encore.Controllers.Internal;

public partial class GatewayController(Session session, IChannelService channelService,
    IChannelSessionManager channelManager, IChannelSessionFactory factory, IOptions<GatewayOptions> options,
    ILogger<GatewayController> logger) : CommandController<Session>(session)
{
    protected ushort GatewayId => (ushort)options.Value.Id;

    protected IReadOnlyList<IChannel?> Channels
    {
        get
        {
            var channels = channelService.GetChannels();
            int count = channels.Count > 0 ? channels.Max(c => c.Id) + 1 : 0;

            return Enumerable.Range(0, count).Select(i => channels.FirstOrDefault(c => c.Id == i)).ToList();
        }
    }

    [CommandHandler]
    public ChannelRegisterResponse ChannelRegister(ChannelRegisterRequest request)
    {
        List<IChannel> registered = [];
        bool failed = false;

        try
        {
            if (Session.Authorized || !channelManager.Validate(Session))
                throw new InvalidOperationException("Session cannot register a channel");

            if (request.Channels.Count == 0)
                throw new InvalidOperationException("Expected at least one channel");

            var address = ((IPEndPoint)Session.Socket.RemoteEndPoint!).Address;
            foreach (var entry in request.Channels)
            {
                if (entry.GatewayId != options.Value.Id)
                    throw new InvalidOperationException($"Gateway Id mismatch (Expected: {options.Value.Id})");

                var endpoint = new IPEndPoint(address, entry.Port);
                var channel  = channelService.CreateChannel(endpoint, entry.ChannelId, entry.Capacity);
                registered.Add(channel);

                logger.LogInformation((int)ServerCommand.ChannelRegister,
                    "Register channel [{SID}/{CID:00}] @ {Address}:{Port}",
                    options.Value.Id, channel.Id, endpoint.Address, endpoint.Port);
            }

            Session.Disconnected += (_, _) =>
            {
                foreach (var channel in registered)
                    channelService.DeleteChannel(channel);
            };

            Session.Authorize(registered.AsReadOnly());
        }
        catch (Exception ex)
        {
            foreach (var channel in registered)
                channelService.DeleteChannel(channel);

            failed = true;
            logger.LogError((int)ServerCommand.ChannelRegister, ex, "Failed to register channel");
        }

        return new ChannelRegisterResponse
        {
            Invalid   = failed,
            GatewayId = GatewayId
        };
    }

    protected async Task<bool> CreateUserChannelSession(int gatewayId, int channelId, CancellationToken cancellationToken)
    {
        var client  = (ClientSession)Session;
        var channel = channelService.FindChannel(channelId);

        if (gatewayId != options.Value.Id || channel?.EndPoint == null || channel.UserCount >= channel.Capacity)
        {
            logger.LogWarning((int)ServerCommand.ChannelLogin,
                "Channel [{GatewayId}/{ChannelId:00}] is unavailable", gatewayId, channelId);

            return false;
        }

        var tcp = new TcpClient();
        try
        {
            await tcp.ConnectAsync(channel.EndPoint, cancellationToken).AsTask()
                .WaitAsync(TimeSpan.FromSeconds(options.Value.Timeout), cancellationToken);

            var session = factory.CreateSession(tcp, client, channel);
            await session.WriteMessage(new CreateSessionRequest
            {
                UserId    = client.Actor.UserId,
                GatewayId = GatewayId,
                ChannelId = (ushort)channelId
            }, cancellationToken);

            client.Register(session);
            channelManager.StartSession(session);

            logger.LogInformation((int)ServerCommand.ChannelLogin,
                "Enter channel [{GatewayId}/{ChannelId:00}]",  gatewayId, channelId);

            return true;
        }
        catch (Exception ex)
        {
            tcp.Dispose();
            logger.LogError((int)ServerCommand.ChannelLogin, ex,
                "Failed to enter channel [{GatewayId}/{ChannelId:00}]", gatewayId, channelId);

            return false;
        }
    }
}
