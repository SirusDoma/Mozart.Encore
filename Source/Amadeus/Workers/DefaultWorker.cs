using System.Net.Sockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Mozart.Options;
using Mozart.Data.Contexts;
using Mozart.Services;
using Mozart.Sessions;

namespace Amadeus;

public class DefaultWorker(IServiceProvider provider, IMozartServer server, ISessionManager manager,
    IMetadataResolver resolver, IChannelService channelService, UserDbContext context, IOptions<DatabaseOptions> dbOptions,
    IOptions<AuthOptions> authOptions, ILogger<DefaultWorker> logger, IHostEnvironment env) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        // Scope in background service
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services#consuming-a-scoped-service-in-a-background-task
        using (var scope = provider.CreateScope())
        using (logger.BeginScope("System"))
        {
            var identityService = scope.ServiceProvider.GetRequiredService<IIdentityService>();

            try
            {
                logger.LogInformation("Amadeus.Encore: Version {Version}", Program.Version);

                // Validate by loading metadata files
                this.ValidateMetadata(channelService, resolver, logger);

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

                // Clear left-over login session or open connection to database.
                if (identityService.Options.RevokeOnStartup)
                    await identityService.ClearSessions(cancellationToken);

                // Start the TCP Server
                server.Start(server.Options.MaxConnections);

                logger.LogInformation("Application started: Listening @ {EndPoint}", server.Socket.LocalEndPoint);
                logger.LogInformation("[!] Network environment: {NetworkVersion} ({Env})", Program.NetworkVersion, env.EnvironmentName);
                logger.LogInformation("[!] Database driver: {Driver}", dbOptions.Value.Driver.GetPrintableName());
                logger.LogInformation("[?] Press [CTRL+C] to shut down");
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Failed to start the application");
                throw;
            }

            manager.Stopped += (sender, args) =>
            {
                logger.LogInformation("Session with [{Client}] has been stopped", args.Session.Socket?.RemoteEndPoint?.ToString()  ??
                    args.Session.GetAuthorizedToken<Actor>().Nickname);

                int expiry = authOptions.Value.SessionExpiry;
                if (expiry > 0)
                {
                    ((ISessionManager)sender!).StartExpiry((Session)args.Session, TimeSpan.FromMinutes(expiry), s =>
                    {
                        // Expiring session after x minutes of disconnection
                        if (s.Channel != null)
                            s.Exit(s.Channel);

                        logger.LogInformation("Deleted login session [{Token}]", s.Actor.Token);
                        identityService.Revoke(s.Actor.Token, CancellationToken.None);
                    });
                }
                else
                {
                    var s = (Session)args.Session;
                    if (s.Channel != null)
                        s.Exit(s.Channel);
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

        using (logger.BeginScope("System"))
            logger.LogInformation("[!] Shutting down application..");

        await manager.ClearSessions();
    }
}