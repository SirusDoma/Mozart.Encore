using Encore.Data.Repositories;
using Encore.Entities;
using Encore.Messages.Requests;
using Encore.Messages.Responses;
using Encore.Messaging;
using Encore.Server;
using Encore.Server.Sessions;
using Encore.Services;
using Encore.Workers.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mozart.Options;
using Mozart.Sessions;

namespace Encore.Controllers.Internal;

public partial class ChannelController(
    Session session,
    IUserRepository userRepository,
    ISessionRepository sessionRepository,
    IAuthService authService,
    IOptions<GatewayOptions> gatewayOptions,
    ICommandDispatcher dispatcher,
    IMessageCodec codec,
    ILogger<ChannelController> logger
    ) : CommandController<Session>(session)
{
    private partial IMessage CreateChannelLoginRequest(ushort planet, ushort channel);

    [CommandHandler]
    public async Task OnChannelRegistered(ChannelRegisterResponse response, CancellationToken cancellationToken)
    {
        if (Session is not GatewaySession gateway || gateway.Authorized)
            return;

        try
        {
            if (response.Invalid)
                throw new InvalidOperationException("Failed to register channel (Check gateway and channel configuration)");

            if (authService.Options.RevokeOnStartup)
                await authService.ClearSessions(gatewayOptions.Value.Id, gatewayOptions.Value.Channels[0].Id,
                    cancellationToken);

            gateway.Authorize(new GatewayInfo { Id = response.GatewayId, PlanetKind = response.PlanetKind });
            logger.LogInformation((int)GameManagerCommand.ChannelRegister,
                "Channel registered with gateway [{GatewayId}]", gateway.Gateway.Id);
        }
        catch (Exception ex)
        {
            logger.LogCritical((int)GameManagerCommand.ChannelRegister, ex, "Failed to register channel");
            gateway.Terminate();
        }
    }

    [CommandHandler]
    public void OnChannelStateUpdated(ChannelStateResponse response)
    {
        logger.LogInformation((int)GameManagerCommand.ChannelState, "Channel load: {Channels}", string.Join(", ",
            response.Channels.Select(c => $"[{c.GatewayId}/{c.ChannelId:00}] {c.Population}/{c.Capacity}")));
    }

    [CommandHandler]
    public async Task CreateSession(CreateSessionRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation((int)GatewayCommand.CreateSession, "Create Session [{UserId}] -> [{SID}/{CID:00}]",
            request.UserId, request.GatewayId, request.ChannelId);

        try
        {
            if (Session.Authorized)
                throw new InvalidOperationException("Session is already created");

            var characterInfo = await userRepository.Find(request.UserId, cancellationToken)
                ?? throw new ArgumentException("Invalid character id");

            var authSession = await sessionRepository.FindByUsername(characterInfo.Username, cancellationToken)
                ?? throw new ArgumentException("Character is not logged in");

            Session.Authorize(new Actor(characterInfo)
            {
                Token    = authSession.Token,
                ClientId = request.Metadata?.ClientId ?? string.Empty
            });
        }
        catch (Exception ex)
        {
            logger.LogWarning((int)GatewayCommand.CreateSession, ex, "Failed to create session [{UserId}]",
                request.UserId);

            Session.Terminate();
            return;
        }

        var login = CreateChannelLoginRequest(request.GatewayId, request.ChannelId);
        await dispatcher.Dispatch(Session, codec.Encode(login), cancellationToken);
    }
}
