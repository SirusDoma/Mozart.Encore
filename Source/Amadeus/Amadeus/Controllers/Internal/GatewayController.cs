using Encore.Server;
using Amadeus;
using Amadeus.Messages.Requests;
using Amadeus.Messages.Responses;

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
            return new ChannelLoginResponse { Failed = true, Info = new ChannelLoginResponse.FailureInfo() };

        return null;
    }
}
