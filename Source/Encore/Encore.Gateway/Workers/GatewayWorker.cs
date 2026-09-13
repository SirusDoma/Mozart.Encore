using System.Net.Sockets;
using Encore.Entities;
using Encore.Messages.Responses;
using Encore.Messaging;
using Encore.Options;
using Encore.Server.Sessions;
using Encore.Services;
using Encore.Workers.Gateway;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mozart.Data.Contexts;
using Mozart.Options;
using Mozart.Sessions;

namespace Encore.Workers;

public partial class GatewayWorker(
    IServiceProvider provider,
    IClientServer clientServer,
    IGatewayServer gatewayServer,
    ISessionManager clientManager,
    IChannelSessionManager channelManager,
    IChannelService channelService,
    MainDbContext context,
    IOptions<DatabaseOptions> dbOptions,
    IOptions<AuthOptions> authOptions,
    IOptions<GatewayOptions> gatewayOptions,
    IMessageCodec codec,
    ILogger<GatewayWorker> logger,
    IHostEnvironment env
) : BackgroundService
{
    private partial Version Version { get; }
    private partial Version NetworkVersion { get; }
    private partial string DatabaseDriverName { get; }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using (logger.BeginScope("System"))
        {
            try
            {
                logger.LogInformation("{Application} (Gateway Mode): Version {Version}", env.ApplicationName, Version);

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
                logger.LogInformation("[!] Network environment: {NetworkVersion} ({Env})", NetworkVersion, env.EnvironmentName);
                logger.LogInformation("[!] Database driver: {Driver}", DatabaseDriverName);
                logger.LogInformation("[?] Press [CTRL+C] to shut down");
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Failed to start the application");
                throw;
            }
        }

        var clientTask    = ExecuteClientServerAsync(cancellationToken);
        var channelTask   = ExecuteChannelServerAsync(cancellationToken);
        var broadcastTask = BroadcastChannelLoadAsync(cancellationToken);

        await Task.WhenAny(clientTask, channelTask);

        using (logger.BeginScope("System"))
            logger.LogInformation("[!] Shutting down application..");

        await Task.WhenAll(clientTask, channelTask, broadcastTask);

        await clientManager.ClearSessions();
        await channelManager.ClearSessions();
    }

    private async Task ExecuteClientServerAsync(CancellationToken cancellationToken)
    {
        // Scope in background service
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services#consuming-a-scoped-service-in-a-background-task
        using var scope = provider.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

        clientManager.Stopped += (sender, args) =>
        {
            var session = (Session)args.Session;
            logger.LogInformation("Session with [{Client}] has been stopped", session.Socket?.RemoteEndPoint?.ToString()
                ?? session.GetAuthorizedToken<Actor>().Nickname);

            int expiry = authOptions.Value.SessionExpiry;
            if (expiry > 0)
            {
                ((ISessionManager)sender!).StartExpiry(session, TimeSpan.FromMinutes(expiry), s =>
                {
                    // Expiring session after 5 minutes of disconnection
                    logger.LogInformation("Deleted login session [{Token}]", s.Actor.Token);
                    if (s.Authorized)
                        authService.Revoke(s.Actor.Token, CancellationToken.None);
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
            if (args.Session is ChannelSession)
                return;

            using (logger.BeginScope("System"))
            using (logger.BeginScope("Channels"))
            {
                logger.LogInformation("Channel session [{Channels}] has been stopped", Describe(args.Session));
            }
        };

        channelManager.Error += (sender, args) =>
        {
            if (args.Session is ChannelSession)
                return;

            using (logger.BeginScope("System"))
            using (logger.BeginScope("Channels"))
            {
                string identifier = Describe(args.Session);
                switch (args.Exception)
                {
                    case EndOfStreamException or IOException { InnerException: SocketException }:
                        logger.LogWarning("Channel [{Identifier}] connection has been lost", identifier);
                        break;
                    case OperationCanceledException or TaskCanceledException:
                        logger.LogInformation("Channel [{Identifier}] terminated by server", identifier);
                        break;
                    default:
                        logger.LogError(args.Exception, "An unxpected error occurred during channel [{Identifier}] execution", identifier);
                        break;
                }
            }
        };

        while (!cancellationToken.IsCancellationRequested)
        {
            Session session;
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
                    logger.LogError(ex, "Failed to accept channel");
                    continue;
                }

                logger.LogInformation("Starting a new channel with [{EndPoint}]", session.Socket.RemoteEndPoint);
            }

            if (!cancellationToken.IsCancellationRequested)
                channelManager.StartSession(session);
            else
                session.Dispose();
        }
    }

    private async Task BroadcastChannelLoadAsync(CancellationToken cancellationToken)
    {
        codec.Register<ChannelStateResponse>();

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(15));
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                var sessions = channelManager.GetSessions()
                    .Where(s => s.Authorized && s.GetAuthorizedToken() is IReadOnlyList<IChannel>).ToList();

                if (sessions.Count == 0)
                    continue;

                byte[] frame = codec.Encode(new ChannelStateResponse
                {
                    Channels = channelService.GetChannels().Select(c => new ChannelStateResponse.ChannelState
                    {
                        GatewayId  = (ushort)gatewayOptions.Value.Id,
                        ChannelId  = (ushort)c.Id,
                        Capacity   = c.Capacity,
                        Population = c.UserCount,
                        Active     = true
                    }).ToList()
                });

                foreach (var session in sessions)
                {
                    try
                    {
                        await session.WriteFrame(frame, cancellationToken);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
        catch (Exception ex) when (ex is OperationCanceledException or TaskCanceledException)
        {
            // Cancellation triggered
        }
    }

    private string Describe(Session session)
        => session.Authorized && session.GetAuthorizedToken() is IReadOnlyList<IChannel> channels
            ? string.Join(", ", channels.Select(c => $"{gatewayOptions.Value.Id}/{c.Id:00}"))
            : session.Socket.RemoteEndPoint?.ToString() ?? "Unregistered";
}
