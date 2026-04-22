using System.Net.Sockets;
using Encore.Server;
using Memoryer.Relay.Sessions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Memoryer.Relay.Workers;

public class RelayWorker(IRelayServerPool pool, IRelaySessionManager manager,
    ICommandDispatcher dispatcher, ILogger<RelayWorker> logger, IHostEnvironment env) : BackgroundService
{
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        using (logger.BeginScope("System"))
        {
            if (pool.Servers.Count == 0)
            {
                logger.LogWarning("Relay is enabled but no endpoints are configured. ");
                await base.StartAsync(cancellationToken);
                return;
            }

            logger.LogInformation("[!] Relay environment: {Env}", env.EnvironmentName);
            foreach (var server in pool.Servers)
            {
                try
                {
                    server.Start(server.Options.MaxConnections);
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.AddressNotAvailable)
                {
                    logger.LogWarning(
                        "Skipping relay endpoint {Address}:{Port} — the address is not assigned to this host.",
                        server.Options.Address, server.Options.Port);
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.AddressAlreadyInUse)
                {
                    logger.LogWarning(
                        "Skipping relay endpoint {Address}:{Port} — the port is already in use",
                        server.Options.Address, server.Options.Port);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex,
                        "Skipping relay endpoint {Address}:{Port} — unexpected error while binding",
                        server.Options.Address, server.Options.Port);
                }
            }

            var servers = pool.Servers.Where(s => s.Active).ToList();
            if (servers.Count == 0)
            {
                logger.LogCritical("Relay failed to bind any of the {Configured} configured endpoint(s)",
                    pool.Servers.Count);
                throw new InvalidOperationException(
                    "Relay failed to bind any configured endpoint. See preceding warnings for details");
            }

            logger.LogInformation("Relay started ({Started}/{Configured} endpoint(s)):",
                servers.Count, pool.Servers.Count);

            foreach (var server in servers)
                logger.LogInformation("  Relay Endpoint: Listening @ {EndPoint}", server.Socket.LocalEndPoint);
        }

        manager.Error += (_, args) =>
        {
            using (logger.BeginScope("System"))
            using (logger.BeginScope("Relay"))
            {
                string address = args.Session.Socket.RemoteEndPoint?.ToString() ?? "unknown";
                switch (args.Exception)
                {
                    case EndOfStreamException or IOException { InnerException: SocketException }:
                        logger.LogWarning("Relay session [{Client}] connection has been lost", address);
                        break;
                    case OperationCanceledException or TaskCanceledException:
                        logger.LogInformation("Relay session [{Client}] terminated by server", address);
                        break;
                    default:
                        logger.LogError(args.Exception,
                            "An unexpected error occurred during relay session [{Client}] execution", address);
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
            logger.LogInformation("[!] Shutting down relay..");

        await manager.ClearSessions();
    }

    private async Task Listen(RelayServer server, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            RelaySession session;
            using (logger.BeginScope("System"))
            using (logger.BeginScope("Relay"))
            {
                try
                {
                    session = await server.AcceptSession(cancellationToken);
                    logger.LogInformation("Accepted a new relay session @ {EndPoint}", server.Socket.LocalEndPoint);

                    await dispatcher.Dispatch(session, RelayCommand.PeerConnected, cancellationToken);
                }
                catch (Exception ex) when (ex is OperationCanceledException or TaskCanceledException)
                {
                    continue;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to accept relay session @ {EndPoint}", server.Socket.LocalEndPoint);
                    continue;
                }

                logger.LogInformation("Starting relay session with [{Client}]", session.Socket.RemoteEndPoint);
            }

            if (!cancellationToken.IsCancellationRequested)
                manager.StartSession(session);
        }
    }
}
