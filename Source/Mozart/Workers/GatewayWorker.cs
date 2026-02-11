using System.Net.Sockets;
using Encore.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mozart.Data.Contexts;
using Mozart.Internal.Requests;
using Mozart.Options;
using Mozart.Services;
using Mozart.Sessions;
using Mozart.Workers.Gateway;

namespace Mozart;

public class GatewayWorker(IServiceProvider provider, IClientServer clientServer, IGatewayServer gatewayServer,
    ISessionManager clientManager, IChannelService channelService, IChannelSessionManager channelManager, IChannelAggregator aggregator,
    UserDbContext context, IOptions<DatabaseOptions> dbOptions, IOptions<AuthOptions> authOptions, IOptions<GatewayOptions> gatewayOptions,
    ILogger<GatewayWorker> logger, IMessageCodec codec, IHostEnvironment env) : BackgroundService
{

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using (logger.BeginScope("System"))
        {
            try
            {
                logger.LogInformation($"Mozart.Encore (Gateway Mode): Version {Program.Version}");

                try
                {
                    // Ensure database and its tables are created when using default auth
                    if (authOptions.Value.Mode == AuthMode.Default)
                        await context.Database.MigrateAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to execute database migration");
                }

                // Start the TCP Server
                clientServer.Start(clientServer.Options.MaxConnections);
                gatewayServer.Start();

                logger.LogInformation("Application started:");
                logger.LogInformation("  Client Endpoint:  Listening @ {EndPoint}", clientServer.Socket.LocalEndPoint);
                logger.LogInformation("  Channel Endpoint: Listening @ {EndPoint}", gatewayServer.Socket.LocalEndPoint);
                logger.LogInformation("[!] Network environment: {NetworkVersion} ({Env})", Program.NetworkVersion, env.EnvironmentName);
                logger.LogInformation("[!] Database driver: {Driver}", dbOptions.Value.Driver.GetPrintableName());
                logger.LogInformation("[?] Press [CTRL+C] to shut down");
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Failed to start the application");
                throw;
            }
        }

        var clientTask = ExecuteClientServerAsync(cancellationToken);
        var channelTask = ExecuteChannelServerAsync(cancellationToken);

        await Task.WhenAny(clientTask, channelTask);

        using (logger.BeginScope("System"))
            logger.LogInformation("[!] Shutting down application..");

        await Task.WhenAll(clientTask, channelTask);

        await clientManager.ClearSessions();
    }

    private async Task ExecuteClientServerAsync(CancellationToken cancellationToken)
    {
        // Scope in background service
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services#consuming-a-scoped-service-in-a-background-task
        using var scope = provider.CreateScope();
        var identityService = scope.ServiceProvider.GetRequiredService<IIdentityService>();

        clientManager.Stopped += (sender, args) =>
        {
            var session = (Session)args.Session;
            logger.LogInformation("Session with [{Client}] has been stopped", session.Socket?.RemoteEndPoint?.ToString()
                ?? session.GetAuthorizedToken<Actor>().Nickname);

            if (session.Properties.TryGetValue("ChannelStatsAggregatorRequestId", out object? obj) && obj is string requestId)
                aggregator.Drop(requestId);

            int expiry = authOptions.Value.SessionExpiry;
            if (expiry > 0)
            {
                ((ISessionManager)sender!).StartExpiry(session, TimeSpan.FromMinutes(expiry), s =>
                {
                    // Expiring session after 5 minutes of disconnection
                    logger.LogInformation("Deleted login session [{Token}]", s.Actor.Token);
                    if (s.Authorized)
                        identityService.Revoke(s.Actor.Token, CancellationToken.None);
                });
            }
        };

        clientManager.Error += (sender, args) =>
        {
            using (logger.BeginScope("System"))
            using (logger.BeginScope("Session"))
            {
                var address = args.Session.Socket?.RemoteEndPoint?.ToString()
                              ?? args.Session.GetAuthorizedToken<Actor>().Nickname;
                switch (args.Exception)
                {
                    case EndOfStreamException or IOException { InnerException: SocketException }:
                        logger.LogWarning("Session [{User}] connection has been lost", address);
                        break;
                    case OperationCanceledException or TaskCanceledException:
                        logger.LogInformation("Session [{User}] terminated by server", address);
                        break;
                    default:
                        logger.LogError(args.Exception,
                            "An unxpected error occurred during session [{User}] execution", address);
                        break;
                }
            }
        };

        while (!cancellationToken.IsCancellationRequested)
        {
            Session session;
            using (logger.BeginScope("System"))
            using (logger.BeginScope("Session"))
            {
                try
                {
                    session = await clientServer.AcceptSession(cancellationToken);
                    logger.LogInformation($"Accepted a new session");
                }
                catch (Exception ex) when (ex is OperationCanceledException or TaskCanceledException)
                {
                    // Cancellation triggered
                    continue;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to accept session");
                    continue;
                }

                logger.LogInformation($"Starting session with [{session.Socket.RemoteEndPoint}]");
            }

            if (!cancellationToken.IsCancellationRequested)
                clientManager.StartSession(session);
        }
    }

    private async Task ExecuteChannelServerAsync(CancellationToken cancellationToken)
    {
        channelManager.Stopped += (sender, args) =>
        {
            var session = (ChannelSession)args.Session;
            channelService.DeleteChannel(session.ChannelId);

            using (logger.BeginScope("System"))
            using (logger.BeginScope("Channels"))
            {
                aggregator.Untrack(session.ChannelId);
                logger.LogInformation("Channel session with [{SID}/{CID:00}] has been stopped",
                    session.GatewayId, session.ChannelId);
            }
        };

        channelManager.Error += (sender, args) =>
        {
            using (logger.BeginScope("System"))
            using (logger.BeginScope("Channels"))
            {
                var session = (ChannelSession)args.Session;
                string identifier = $"{session.GatewayId}/{session.ChannelId:00}";
                switch (args.Exception)
                {
                    case EndOfStreamException or IOException { InnerException: SocketException }:
                        logger.LogWarning("Channel [{Identifier}] connection has been lost", identifier);
                        break;
                    case OperationCanceledException or TaskCanceledException:
                        logger.LogInformation("Channel [{Identifier}] terminated by server", identifier);
                        break;
                    default:
                        logger.LogError(args.Exception,
                            "An unxpected error occurred during channel [{Identifier}] execution", identifier);
                        break;
                }
            }
        };

        while (!cancellationToken.IsCancellationRequested)
        {
            ChannelSession session;
            using (logger.BeginScope("System"))
            using (logger.BeginScope("Channels"))
            {
                try
                {
                    session = await gatewayServer.AcceptSession(cancellationToken);
                    logger.LogInformation($"A new channel discovered");
                }
                catch (Exception ex) when (ex is OperationCanceledException or TaskCanceledException)
                {
                    // Cancellation triggered
                    continue;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to register channel");
                    continue;
                }

                logger.LogInformation("Starting a new channel with [{EndPoint}]", session.Socket.RemoteEndPoint);
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    if (!cancellationToken.IsCancellationRequested)
                        channelManager.StartSession(session);

                    await session.WaitForAuthorization(TimeSpan.FromSeconds(gatewayOptions.Value.Timeout), cancellationToken);
                    channelManager.Authorize(session);

                    using (logger.BeginScope("System"))
                    using (logger.BeginScope("Channels"))
                    {
                        logger.LogInformation("Channel [{SID}/{CID:00}] registered @ [{EndPoint}]",
                            session.GatewayId, session.ChannelId, session.Socket.RemoteEndPoint);
                    }
                }
                catch (Exception ex)
                {
                    using (logger.BeginScope("System"))
                    using (logger.BeginScope("Channels"))
                    {
                        logger.LogError(ex, "Failed to start channel [{SID}/{CID:00}]",
                            session.GatewayId, session.ChannelId);
                    }

                    await session.WriteFrame(codec.Encode(new CreateChannelResponse()
                    {
                        Success = false
                    }), cancellationToken);

                    await channelManager.StopSession(session);
                }
            }, cancellationToken);
        }
    }

}
