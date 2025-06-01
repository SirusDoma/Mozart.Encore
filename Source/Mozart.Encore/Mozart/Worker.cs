using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Encore.Sessions;
using Encore.Server;

namespace Mozart;

public class Worker(ITcpServer server, SessionManager manager, ILogger<Worker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        server.Start();
        logger.LogInformation($"Listening {server.Socket.LocalEndPoint}");

        manager.Error += (sender, args) =>
        {
            logger.LogError(args.Exception, "An error occurred during session execution");
        };

        while (!cancellationToken.IsCancellationRequested)
        {
            Session session;

            try
            {
                session = await server.AcceptSession(cancellationToken);
            }
            catch (TaskCanceledException)
            {
                // Cancellation triggered
                continue;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to accept session");
                continue;
            }

            logger.LogInformation($"Starting session for {session.Socket.RemoteEndPoint}.");
            manager.StartSession(session);
        }
    }
}