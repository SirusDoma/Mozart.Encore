using Encore.Messaging;
using Mozart.Messages.Requests;

namespace Encore.Controllers.Internal;

public partial class ChannelController
{
    private partial IMessage CreateChannelLoginRequest(ushort planet, ushort channel) => new ChannelLoginRequest
    {
        GatewayId = (short)planet,
        ChannelId = (short)channel
    };
}
