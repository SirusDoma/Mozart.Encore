using System.Net.Sockets;
using Encore.Entities;
using Encore.Messages.Requests;
using Encore.Messaging;
using Encore.Server;
using Encore.Server.Sessions;
using Encore.Services;
using Microsoft.Extensions.Options;
using Mozart.Options;

namespace Encore.Workers.Channels;

public class GatewaySession(
    TcpClient client, IOptions<TcpOptions> options,
    IMessageFramerFactory framer,
    ICommandDispatcher dispatcher,
    IMessageCodec codec,
    IChannelService channelService,
    IOptions<GatewayOptions> gatewayOptions
) : Session(client, options, framer, dispatcher, codec)
{
    public GatewayInfo Gateway => GetAuthorizedToken<GatewayInfo>();

    public override async Task Execute(CancellationToken cancellationToken)
    {
        await SendRegisterChannelRequest(cancellationToken);
        await base.Execute(cancellationToken);
    }

    protected virtual async Task SendRegisterChannelRequest(CancellationToken cancellationToken)
    {
        await WriteMessage(new ChannelRegisterRequest
        {
            Channels = channelService.GetChannels().Select(channel => new ChannelRegisterRequest.ChannelEntry
            {
                GatewayId = (ushort)gatewayOptions.Value.Id,
                ChannelId = (ushort)channel.Id,
                Port      = Options.Port,
                Capacity  = channel.Capacity,
                Active    = true
            }).ToList()
        }, cancellationToken);
    }
}
