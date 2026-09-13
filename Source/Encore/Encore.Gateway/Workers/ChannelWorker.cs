using System.Net;
using System.Net.Sockets;
using Encore.Metadata;
using Encore.Options;
using Encore.Server;
using Encore.Server.Sessions;
using Encore.Services;
using Encore.Workers.Channels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mozart.Data.Contexts;
using Mozart.Options;
using Mozart.Sessions;

namespace Encore.Workers;

public partial class ChannelWorker(
    IServiceProvider provider,
    IHostApplicationLifetime lifetime,
    ITcpServer<Session> server,
    ISessionManager manager,
    IGatewaySessionFactory factory,
    IMetadataResolver resolver,
    IChannelService channelService,
    MainDbContext context,
    IOptions<DatabaseOptions> dbOptions,
    IOptions<AuthOptions> authOptions,
    IOptions<GatewayOptions> gatewayOptions,
    ILogger<ChannelWorker> logger,
    IHostEnvironment env
) : BackgroundService
{
    private partial Version Version { get; }
    private partial Version NetworkVersion { get; }
    private partial string DatabaseDriverName { get; }
    private partial void ValidateChannelMetadata();

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        // Scope in background service
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services#consuming-a-scoped-service-in-a-background-task
        using var scope = provider.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

        GatewaySession gateway;
        Task execution;

        using (logger.BeginScope("System"))
        {
            try
            {
                logger.LogInformation("{Application} (Channel Mode): Version {Version}", env.ApplicationName, Version);

                // Validate config
                if (gatewayOptions.Value.Channels.Count != 1)
                    throw new InvalidOperationException("[Gateway:Channels] must contains exactly 1 channel configuration");

                // Validate by loading metadata files
                ValidateChannelMetadata();

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

                // Connect to the Gateway Server
                var timeout = TimeSpan.FromSeconds(gatewayOptions.Value.Timeout);
                var tcp     = new TcpClient();

                try
                {
                    await tcp.ConnectAsync(IPAddress.Parse(gatewayOptions.Value.Address), gatewayOptions.Value.Port, cancellationToken)
                        .AsTask().WaitAsync(timeout, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogCritical(ex, "Failed to connect to the gateway server");
                    throw;
                }


                server.Start(server.Options.MaxConnections);

                gateway   = factory.CreateSession(tcp);
                execution = RunGatewaySessionAsync(gateway, cancellationToken);

                logger.LogInformation("Application started:");
                logger.LogInformation("  Gateway Endpoint: Connected @ {Endpoint}", gateway.Socket.RemoteEndPoint);
                logger.LogInformation("  Session Endpoint:  Listening @ {EndPoint}", server.Socket.LocalEndPoint);
                logger.LogInformation("[!] Network environment: {NetworkVersion} ({Env})", NetworkVersion,
                    env.EnvironmentName);
                logger.LogInformation("[!] Database driver: {Driver}", DatabaseDriverName);
                logger.LogInformation("[?] Press [CTRL+C] to shut down");
            }
            catch (Exception ex) when (ex is SocketException or IOException { InnerException: SocketException })
            {
                logger.LogCritical(ex, "Failed to connect to the gateway server");
                throw;
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Failed to start the application");
                throw;
            }

            manager.Stopped += (sender, args) =>
            {
                var session = (Session)args.Session;
                logger.LogInformation("Session with [{Client}] has been stopped", session.Socket?.RemoteEndPoint?.ToString()
                    ?? session.GetAuthorizedToken<Actor>().Nickname);

                int expiry = authOptions.Value.SessionExpiry;
                if (expiry > 0)
                {
                    ((ISessionManager)sender!).StartExpiry(session, TimeSpan.FromMinutes(expiry), s =>
                    {
                        // Expiring session after x minutes of disconnection
                        if (s.Channel != null)
                            s.Exit(s.Channel);

                        logger.LogInformation("Deleted login session [{Token}]", s.Actor.Token);
                        authService.Revoke(s.Actor.Token, CancellationToken.None);
                    });
                }
                else if (session.Channel != null)
                {
                    session.Exit(session.Channel);
                }
            };

            manager.Error += (sender, args) =>
            {
                using (logger.BeginScope("System"))
                using (logger.BeginScope("Session"))
                {
                    var address = args.Session.Socket?.RemoteEndPoint?.ToString() ??
                                  args.Session.GetAuthorizedToken<Actor>().Nickname;
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
        }

        var accept = AcceptSessionsAsync(cancellationToken);
        await Task.WhenAny(accept, execution);

        using (logger.BeginScope("System"))
            logger.LogInformation("[!] Shutting down application..");

        gateway.Terminate();
        await manager.ClearSessions();
        lifetime.StopApplication();
    }

    private async Task AcceptSessionsAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            Session session;
            using (logger.BeginScope("System"))
            using (logger.BeginScope("Session"))
            {
                try
                {
                    session = await server.AcceptSession(cancellationToken);
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
                manager.StartSession(session);
        }
    }

    private async Task RunGatewaySessionAsync(GatewaySession gateway, CancellationToken cancellationToken)
    {
        try
        {
            await gateway.Execute(cancellationToken);
        }
        catch (Exception ex) when (ex is OperationCanceledException or TaskCanceledException)
        {
            // Cancellation triggered
        }
        catch (Exception ex) when (ex is EndOfStreamException or SocketException or ObjectDisposedException
                                       or IOException { InnerException: SocketException })
        {
            using (logger.BeginScope("System"))
                logger.LogCritical("Connection to the gateway has been lost");
        }
        catch (Exception ex)
        {
            using (logger.BeginScope("System"))
                logger.LogCritical(ex, "An unexpected error occurred on the gateway session");
        }
    }
}
