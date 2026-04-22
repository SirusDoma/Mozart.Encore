using System.Net;
using System.Security.Cryptography;
using Encore.Server;
using Memoryer.Relay.Messages.Events;
using Memoryer.Relay.Messages.Requests;
using Memoryer.Relay.Services;
using Memoryer.Relay.Sessions;
using Microsoft.Extensions.Logging;

namespace Memoryer.Relay.Controllers;

public class RelayController(
    RelaySession session,
    IRelayServerPool pool,
    IRelaySessionManager manager,
    IGameSessionService gameSessions,
    ILogger<RelayController> logger
) : CommandController<RelaySession>(session)
{
    [CommandHandler(RelayCommand.PeerConnected)]
    public async Task Connect(CancellationToken cancellationToken)
    {
        logger.LogInformation((int)RelayCommand.PeerConnected, "Relay session created");

        Session.Authorize(new RelayActor
        {
            SessionKey1    = BitConverter.ToInt32(RandomNumberGenerator.GetBytes(4)),
            SessionKey2    = BitConverter.ToInt32(RandomNumberGenerator.GetBytes(4)),
            LocalEndpoint  = Session.LocalEndPoint,
            PublicEndpoint = Session.RemoteEndPoint,
            RelayServer    = pool.Servers
                .Where(s => s.Active)
                .Select(s => new IPEndPoint(IPAddress.Parse(s.Options.Address), s.Options.Port))
                .First()
        });

        var client = Session.GetAuthorizedToken<RelayActor>();
        await Session.WriteMessage(new PeerConnectedEventData
        {
            RelayEndpoints = Enumerable.Repeat(client.RelayServer, 3).ToList(),
            SessionKey1    = client.SessionKey1,
            SessionKey2    = client.SessionKey2,
        }, cancellationToken);

        await Session.WriteMessage(new PeerEndpointAssignedEventData
        {
            RelayEndpoints = Enumerable.Repeat(client.RelayServer, 3).ToList(),
            PublicEndpoint = Session.RemoteEndPoint,
        }, cancellationToken);
    }

    [CommandHandler]
    public async Task Confirm(PeerConfirmRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation((int)RelayCommand.PeerConfirm,
            "Confirm: {EP} ({LEP})", request.PublicEndpoint, request.LocalEndpoint);

        var client = Session.GetAuthorizedToken<RelayActor>();
        client.LocalEndpoint  = request.LocalEndpoint;
        client.PublicEndpoint = request.PublicEndpoint;
        await Session.WriteMessage(new PeerEndpointAssignedEventData
        {
            RelayEndpoints = Enumerable.Repeat(client.RelayServer, 3).ToList(),
            PublicEndpoint = request.PublicEndpoint,
        }, cancellationToken);
    }

    [CommandHandler]
    public async Task UpdateGameStats(UpdateGameStatsEventData ev, CancellationToken cancellationToken)
    {
        var actor = Session.GetAuthorizedToken<RelayActor>();
        logger.LogInformation((int)RelayCommand.UpdateGameStats,
            "Update game stats: {EP}", actor.PublicEndpoint);

        var game = gameSessions.GetGameSession(actor.GameSessionId);
        if (game == null)
            return;

        foreach (var member in game.Sessions)
        {
            if (member != Session)
                await member.WriteMessage(ev, cancellationToken);
        }
    }

    [CommandHandler]
    public async Task DisablePlay(ComboBrokenEventData ev, CancellationToken cancellationToken)
    {
        var actor = Session.GetAuthorizedToken<RelayActor>();
        logger.LogInformation((int)RelayCommand.ComboBroken,
            "Combo break: {EP}", actor.PublicEndpoint);

        var game = gameSessions.GetGameSession(actor.GameSessionId);
        if (game == null)
            return;

        foreach (var member in game.Sessions)
        {
            if (member != Session)
                await member.WriteMessage(ev, cancellationToken);
        }
    }

    [CommandHandler]
    public async Task EnablePlay(ComboStartedEventData ev, CancellationToken cancellationToken)
    {
        var actor = Session.GetAuthorizedToken<RelayActor>();
        logger.LogInformation((int)RelayCommand.ComboStarted,
            "Combo start: {EP}", actor.PublicEndpoint);

        var game = gameSessions.GetGameSession(actor.GameSessionId);
        if (game == null)
            return;

        foreach (var member in game.Sessions)
        {
            if (member != Session)
                await member.WriteMessage(ev, cancellationToken);
        }
    }

    [CommandHandler]
    public void Ping(PingRequest request)
    {
        // logger.LogInformation((int)RelayCommand.Ping,
        //     "Ping: {P1}/{P2}", request.Start, request.Tick);
    }

    [CommandHandler]
    public void CreateSession(CreateRelaySessionRequest request)
    {
        var matched = request.Members
            .Select(m => manager.FindByKeys(m.SessionKey1, m.SessionKey2))
            .Where(s => s is not null)
            .Cast<RelaySession>()
            .Distinct()
            .ToList();

        if (matched.Count == 0)
            return;

        if (matched.Any(s => gameSessions.GetGameSession(s.GetAuthorizedToken<RelayActor>().GameSessionId) != null))
            return;

        logger.LogInformation((int)RelayCommand.CreateSession,
            "CreateSession: {Count} member(s)", request.Members.Count);

        gameSessions.CreateGameSession(matched);
    }

    [CommandHandler]
    public void DeleteSession(DeleteRelaySessionRequest request)
    {
        var matched = request.Members
            .Select(m => manager.FindByKeys(m.SessionKey1, m.SessionKey2))
            .Where(s => s is not null)
            .Cast<RelaySession>()
            .ToHashSet();

        if (matched.Count == 0)
            return;

        if (matched.All(s => gameSessions.GetGameSession(s.GetAuthorizedToken<RelayActor>().GameSessionId) == null))
            return;

        logger.LogInformation((int)RelayCommand.CreateSession,
            "DeleteSession: {Count} member(s)", request.Members.Count);

        foreach (var member in matched)
        {
            gameSessions.DeleteGameSession(member.GetAuthorizedToken<RelayActor>().GameSessionId);
            member.GetAuthorizedToken<RelayActor>().GameSessionId = 0;
        }


    }
}
