using System.Net.Sockets;
using Encore.Sessions;
using Memoryer.Relay.Sessions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Memoryer.Relay.Workers;

public class UdpRelayWorker(
    IUdpRelayServerPool pool,
    IUdpSessionManager<UdpRelaySession> manager,
    ILogger<UdpRelayWorker> logger,
    IHostEnvironment env
) : BackgroundService
{
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        using (logger.BeginScope("System"))
        {
            if (pool.Servers.Count == 0)
            {
                logger.LogWarning("UDP relay is enabled but no endpoints are configured.");
                await base.StartAsync(cancellationToken);
                return;
            }

            logger.LogInformation("[!] UDP relay environment: {Env}", env.EnvironmentName);
            foreach (var server in pool.Servers)
            {
                try
                {
                    server.Start();
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.AddressNotAvailable)
                {
                    logger.LogWarning(
                        "Skipping UDP relay endpoint {Address}:{Port} — the address is not assigned to this host.",
                        server.Options.Address, server.Options.Port);
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.AddressAlreadyInUse)
                {
                    logger.LogWarning(
                        "Skipping UDP relay endpoint {Address}:{Port} — the port is already in use",
                        server.Options.Address, server.Options.Port);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex,
                        "Skipping UDP relay endpoint {Address}:{Port} — unexpected error while binding",
                        server.Options.Address, server.Options.Port);
                }
            }

            var servers = pool.Servers.Where(s => s.Active).ToList();
            if (servers.Count == 0)
            {
                logger.LogWarning("UDP relay failed to bind any of the {Configured} configured endpoint(s)",
                    pool.Servers.Count);

                return;
            }

            logger.LogInformation("UDP relay started ({Started}/{Configured} endpoint(s)):",
                servers.Count, pool.Servers.Count);

            foreach (var server in servers)
                logger.LogInformation("  UDP Relay Endpoint: Listening @ {EndPoint}", server.Client.Client.LocalEndPoint);
        }

        manager.Error += (_, args) =>
        {
            using (logger.BeginScope("System"))
            using (logger.BeginScope("UdpRelay"))
            {
                string address = args.Session.RemoteEndPoint.ToString();
                switch (args.Exception)
                {
                    case OperationCanceledException or TaskCanceledException:
                        logger.LogInformation("UDP relay datagram from [{Client}] cancelled", address);
                        break;
                    default:
                        logger.LogError(args.Exception,
                            "An unexpected error occurred during UDP relay datagram dispatch from [{Client}]", address);
                        break;
                }
            }
        };

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var started = pool.Servers.Where(s => s.Active).ToList();
        if (started.Count == 0)
            return;

        var acceptTasks = started
            .Select(server => Listen(server, cancellationToken))
            .ToArray();

        await Task.WhenAll(acceptTasks);

        using (logger.BeginScope("System"))
            logger.LogInformation("[!] Shutting down UDP relay..");
    }

    private async Task Listen(UdpRelayServer server, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            UdpRelaySession session;
            using (logger.BeginScope("System"))
            using (logger.BeginScope("UdpRelay"))
            {
                try
                {
                    session = await server.AcceptSession(cancellationToken);
                }
                catch (Exception ex) when (ex is OperationCanceledException or TaskCanceledException)
                {
                    continue;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to accept UDP relay datagram @ {EndPoint}",
                        server.Client.Client.LocalEndPoint);

                    continue;
                }
            }

            if (!cancellationToken.IsCancellationRequested)
                manager.StartSession(session);
        }
    }
}
