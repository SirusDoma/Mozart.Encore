using CrossTime.Messages.Requests;
using CrossTime.Messages.Responses;
using Encore.Server;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mozart.Options;
using Mozart.Services;
using Mozart.Sessions;

namespace CrossTime.Controllers;

[Authorize]
public class PlanetController(
    Session session, ISessionManager manager,
    IAuthService authService,
    IChannelService channelService,
    IOptions<ServerOptions> serverOptions,
    IOptions<GatewayOptions> options,
    ILogger<PlanetController> logger
) : CommandController<Session>(session)
{
    // There could be 6 server at max and every server has different part of segment in the channel payload
    // Each could have 20 channels at max
    private static readonly Dictionary<int, int> ChannelStartIndices = new()
    {
        { 1, 20 },
        { 2, 40 },
        { 3, 60 },
        { 4, 80 },
        { 5, 100 },
        { 6, 120 },
    };

    private const int MaxChannels = 200;

    [CommandHandler(RequestCommand.GetChannelList)]
    public ChannelListResponse GetChannelList()
    { ;
        var gateway = options.Value;
        logger.LogInformation(
            (int)RequestCommand.GetChannelList,
            "Get channel list: [{GatewayId}]",
            Session.Actor.ServerId
        );

        var states   = new List<ChannelListResponse.ChannelState>();
        var channels = channelService.GetChannels();

        if (!ChannelStartIndices.TryGetValue(Session.Actor.ServerId, out int startIndex))
            startIndex = 1;

        if (channels.Count > 0)
        {
            for (ushort i = 0; i < MaxChannels; i++)
            {
                var channel = channels.SingleOrDefault(s => s.Id + startIndex == i);
                states.Add(new ChannelListResponse.ChannelState
                {
                    ServerId   = (ushort)Session.Actor.ServerId,
                    ChannelId  = (ushort)(i - startIndex),
                    Capacity   = channel?.Capacity ?? 100,
                    Population = channel?.UserCount ?? 0,
                    Active     = channel != null
                });
            }
        }

        return new ChannelListResponse { Channels = states };
    }

    [CommandHandler]
    public async Task<ChannelLoginResponse> ChannelLogin(ChannelLoginRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            (int)RequestCommand.ChannelLogin,
            "Enter channel: [{GatewayId}/{ChannelId:00}]",
            request.ServerId,
            request.ChannelId
        );

        if (serverOptions.Value.Mode != DeploymentMode.Full && request.ServerId != options.Value.Id)
            throw new ArgumentOutOfRangeException(nameof(request), "Invalid gateway server id");

        const StringComparison comparison = StringComparison.InvariantCultureIgnoreCase;
        var existingSession = channelService.Sessions.FirstOrDefault(s => s.Actor.Token.Equals(Session.Actor.Token, comparison));
        if (existingSession != null)
        {
            if (!manager.Validate(existingSession))
            {
                if (existingSession.Channel != null)
                    existingSession.Exit(existingSession.Channel!);

                if (existingSession.Room != null)
                    existingSession.Exit(existingSession.Room!);
            }
            else
            {
                await Session.WriteMessage(new AuthResponse
                {
                    Result = AuthSessionResult.DuplicateSessions
                }, cancellationToken);

                return null!;
            }
        }

        try
        {
            var channel = channelService.GetChannel(request.ChannelId);
            Session.Register(channel);

            await authService.UpdateChannel(Session.Actor.Token, request.ServerId, request.ChannelId, cancellationToken);
            return new ChannelLoginResponse
            {
                Failed    = false,
                ErrorCode = LoginErrorCode.Undefined,
                Ranking   = Session.Actor.Ranking
            };
        }
        catch (InvalidOperationException)
        {
            return new ChannelLoginResponse
            {
                Failed    = true,
                ErrorCode = LoginErrorCode.Undefined,
                Ranking   = 0
            };
        }
    }
}
