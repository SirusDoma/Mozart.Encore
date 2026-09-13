using Encore.Server;
using Mozart;
using Mozart.Messages.Requests;
using Mozart.Messages.Responses;

namespace Encore.Controllers.Internal;

public partial class GatewayController
{
    [CommandHandler(RequestCommand.GetChannelList)]
    [Authorize]
    public ChannelListResponse GetChannelList() => new()
    {
        Channels = Channels.Select((c, i) => new ChannelListResponse.ChannelState
        {
            ServerId  = (ushort)(c != null ? ServerId : 0),
            ChannelId  = (ushort)i,
            Capacity   = c?.Capacity  ?? 0,
            Population = c?.UserCount ?? 0,
            Active     = c != null
        }).ToList()
    };

    [CommandHandler]
    [Authorize]
    public async Task<ChannelLoginResponse?> ChannelLogin(ChannelLoginRequest request,
        CancellationToken cancellationToken)
    {
        if (!await CreateUserChannelSession(request.ServerId, request.ChannelId, cancellationToken))
            return new ChannelLoginResponse { Full = true };

        return null;
    }
}
