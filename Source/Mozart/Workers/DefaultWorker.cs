using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Mozart.Options;
using Mozart.Data.Contexts;
using Mozart.Services;
using Mozart.Sessions;

namespace Mozart;

public class DefaultWorker(IServiceProvider provider, IMozartServer server, SessionManager manager, IMetadataResolver resolver,
    IChannelService channelService, UserDbContext context, IOptions<DatabaseOptions> dbOptions, IOptions<AuthOptions> authOptions,
    ILogger<DefaultWorker> logger, IHostEnvironment env) : BackgroundService
{
    public static string GatewayId { get; } = Guid.NewGuid().ToString().ToUpper();

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
                logger.LogInformation($"Mozart.Encore: Version {Program.Version}");

                // Ensure database and its tables are created
                await context.Database.EnsureCreatedAsync(cancellationToken);

                // Clear left-over login session or open connection to database.
                if (identityService.Options.RevokeOnStartup)
                    await identityService.ClearSessions(cancellationToken);

                // Validate by loading metadata files
                ValidateMetadata();

                // Start the TCP Server
                server.Start(server.Options.MaxConnections);

                logger.LogInformation($"Application started: Listening @ {server.Socket.LocalEndPoint}");
                logger.LogInformation($"[!] Network environment: {Program.NetworkVersion} ({env.EnvironmentName})");
                logger.LogInformation($"[!] Database driver: {dbOptions.Value.Driver.GetPrintableName()}");
                logger.LogInformation($"[?] Press [CTRL+C] to shut down");
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Failed to start the application");
                throw;
            }

            manager.Stopped += (sender, args) =>
            {
                logger.LogInformation("Session with [{Client}] has been stopped", args.Session.Socket.RemoteEndPoint);

                int expiry = authOptions.Value.SessionExpiry;
                if (expiry > 0)
                {
                    ((SessionManager)sender!).StartExpiry((Session)args.Session, TimeSpan.FromMinutes(expiry), s =>
                    {
                        // Expiring session after 5 minutes of disconnection
                        logger.LogInformation("Deleted login session [{Token}]", s.Actor.Token);
                        identityService.Revoke(s.Actor.Token, CancellationToken.None);
                    });
                }
            };

            manager.Error += (sender, args) =>
            {
                using (logger.BeginScope("System"))
                using (logger.BeginScope("Session"))
                {
                    var address = ((Session?)sender)?.Socket.RemoteEndPoint;
                    switch (args.Exception)
                    {
                        case EndOfStreamException:
                            logger.LogWarning("Session [{User}] connection has been lost", address);
                            break;
                        case OperationCanceledException or TaskCanceledException:
                            logger.LogWarning("Session [{User}] terminated by server", address);
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

    private void ValidateMetadata()
    {
        try
        {
            foreach (var channel in channelService.GetChannels())
                _ = resolver.GetItemData(channel);
        }
        catch (Exception)
        {
            logger.LogError("Failed to validate metadata files");
            throw;
        }
    }
}