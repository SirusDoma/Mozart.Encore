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
using Mozart.Workers.Channels;

namespace Mozart;

public class ChannelWorker(IServiceProvider provider, IHostApplicationLifetime lifetime, IGatewayClient gatewayClient,
    ISessionManager manager, IMetadataResolver resolver, IChannelService channelService, UserDbContext context,
    IOptions<DatabaseOptions> dbOptions, IOptions<AuthOptions> authOptions, IOptions<GatewayOptions> gatewayOptions,
    ILogger<ChannelWorker> logger, IHostEnvironment env) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        // Scope in background service
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services#consuming-a-scoped-service-in-a-background-task
        using (var scope = provider.CreateScope())
        using (var _ = logger.BeginScope("System"))
        {
            var identityService = scope.ServiceProvider.GetRequiredService<IIdentityService>();

            try
            {
                logger.LogInformation($"Mozart.Encore (Channel Mode): Version {Program.Version}");

                // Validate config
                if (gatewayOptions.Value.Channels.Count != 1)
                    throw new InvalidOperationException(
                        "[Gateway:Channels] must contains exactly 1 channel configuration");

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

                // Connect to the Gateway Server
                await gatewayClient.Connect(cancellationToken);

                // Clear left-over login session or open connection to database.
                if (identityService.Options.RevokeOnStartup)
                {
                    await identityService.ClearSessions(
                        gatewayOptions.Value.Id,
                        gatewayOptions.Value.Channels.First().Id,
                        cancellationToken
                    );
                }

                logger.LogInformation("Application started: Connected to Gateway @ {Endpoint}",
                    gatewayClient.Socket.RemoteEndPoint);
                logger.LogInformation("[!] Network environment: {NetworkVersion} ({Env})", Program.NetworkVersion,
                    env.EnvironmentName);
                logger.LogInformation("[!] Database driver: {Driver}", dbOptions.Value.Driver.GetPrintableName());
                logger.LogInformation("[?] Press [CTRL+C] to shut down");
            }
            catch (IOException ex) when (ex.InnerException is SocketException)
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
                logger.LogInformation("Session with [{Client}] has been stopped", args.Session.Socket?.RemoteEndPoint);
            };

            manager.Error += (sender, args) =>
            {
                using (logger.BeginScope("System"))
                using (logger.BeginScope("Session"))
                {
                    var address = args.Session.Socket?.RemoteEndPoint;
                    switch (args.Exception)
                    {
                        case EndOfStreamException or IOException { InnerException: SocketException }:
                            logger.LogCritical("Connection to the gateway has been lost");
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
            var accept = gatewayClient.AcceptSession(cancellationToken);

            using (logger.BeginScope("System"))
            using (logger.BeginScope("Session"))
            {
                try
                {
                    session = await accept;
                    logger.LogInformation($"Accepted a new session");
                }
                catch (Exception ex) when (ex is OperationCanceledException or TaskCanceledException)
                {
                    // Cancellation triggered
                    continue;
                }
                catch (Exception ex) when (ex is EndOfStreamException or IOException { InnerException: SocketException })
                {
                    logger.LogWarning("Connection to the gateway has been lost");
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to accept session");
                    continue;
                }

                logger.LogInformation("Starting session with [{Client}]", session.Socket.RemoteEndPoint);
            }

            if (!cancellationToken.IsCancellationRequested)
                manager.StartSession(session);
        }

        using (logger.BeginScope("System"))
            logger.LogInformation("[!] Shutting down application..");

        try
        {
            await gatewayClient.SendGatewayCommand(ChannelCommand.DeleteChannel, CancellationToken.None);
        }
        catch (Exception ex) when (ex is EndOfStreamException or IOException { InnerException: SocketException })
        {
            logger.LogWarning("Failed to send shutdown signal to the gateway server");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send shutdown signal to the gateway server");
        }

        await manager.ClearSessions();
        lifetime.StopApplication();
    }
} 