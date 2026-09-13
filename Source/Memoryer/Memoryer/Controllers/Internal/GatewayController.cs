using Encore.Server;
using Memoryer;
using Memoryer.Messages.Requests;
using Memoryer.Messages.Responses;

namespace Encore.Controllers.Internal;

public partial class GatewayController
{
    [CommandHandler(RequestCommand.GetChannelList)]
    [Authorize]
    public ChannelListResponse GetChannelList() => new()
    {
        Channels = Channels.Select((c, i) => new ChannelListResponse.ChannelState
        {
            GatewayId  = (ushort)(c != null ? GatewayId : 0),
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
        if (!await CreateUserChannelSession(request.GatewayId, request.ChannelId, cancellationToken))
            return new ChannelLoginResponse { Result = ChannelLoginResult.ChannelUnavailable };

        return null;
    }
}
