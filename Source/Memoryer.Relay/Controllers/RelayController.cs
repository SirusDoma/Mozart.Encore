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
    IRelayPeer session,
    IUdpRelayServerPool serverPool,
    IRelaySessionLocator locator,
    IGameSessionService gameSessions,
    ILogger<RelayController> logger
) : CommandController<IRelayPeer>(session)
{
    [CommandHandler(RelayCommand.PeerConnected)]
    public async Task Connect(CancellationToken cancellationToken)
    {
        logger.LogInformation((int)RelayCommand.PeerConnected, "Relay session created");

        if (!Session.Authorized)
        {
            Session.Authorize(new RelayActor
            {
                SessionKey1    = BitConverter.ToInt32(RandomNumberGenerator.GetBytes(4)),
                SessionKey2    = BitConverter.ToInt32(RandomNumberGenerator.GetBytes(4)),
                LocalEndpoint  = new IPEndPoint(IPAddress.None, 0),
                PublicEndpoint = Session.RemoteEndPoint,
                RelayServer    = Session.LocalEndPoint
            });
        }
        else
            Session.GetAuthorizedToken<RelayActor>().RelayServer = Session.LocalEndPoint;

        var actor = Session.GetAuthorizedToken<RelayActor>();
        await Session.WriteMessage(new PeerConnectedEventData
        {
            RelayEndpoints = Enumerable.Repeat(actor.RelayServer, 3).ToList(),
            SessionKey1    = actor.SessionKey1,
            SessionKey2    = actor.SessionKey2,
        }, cancellationToken);

        // Sending PeerEndpointAssignedEventData will force the game to fall back to TCP relay instead of peer-to-peer.
        if (serverPool.Servers.Count == 0)
        {
            await Session.WriteMessage(new PeerEndpointAssignedEventData
            {
                RelayEndpoints = Enumerable.Repeat(actor.RelayServer, 3).ToList(),
                PublicEndpoint = Session.RemoteEndPoint,
            }, cancellationToken);
        }
    }

    [CommandHandler]
    public async Task Confirm(PeerConfirmRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation((int)RelayCommand.PeerConfirm,
            "Confirm: {EP} ({LEP}): {AEP}", request.PublicEndpoint, request.LocalEndpoint, Session.RemoteEndPoint);

        RelayActor actor;
        if (Session.Authorized)
        {
            actor = Session.GetAuthorizedToken<RelayActor>();
            if (actor.SessionKey1 != request.SessionKey1 && actor.SessionKey2 != request.SessionKey2)
            {
                logger.LogInformation((int)RelayCommand.PeerConfirm,
                    "  Confirm: Re-assigning session");

                actor.SessionKey1 = request.SessionKey1;
                actor.SessionKey2 = request.SessionKey2;
            }

            actor.LocalEndpoint  = request.LocalEndpoint;
            actor.PublicEndpoint = request.PublicEndpoint;
        }
        else
        {
            Session.Authorize(new RelayActor
            {
                SessionKey1    = request.SessionKey1,
                SessionKey2    = request.SessionKey2,
                LocalEndpoint  = new IPEndPoint(IPAddress.None, 0),
                PublicEndpoint = Session.RemoteEndPoint,
                RelayServer    = Session.LocalEndPoint
            });

            actor = Session.GetAuthorizedToken<RelayActor>();
            logger.LogInformation("  Using relay {EP}", actor.RelayServer);
        }

        await Session.WriteMessage(new PeerEndpointAssignedEventData
        {
            RelayEndpoints = Enumerable.Repeat(actor.RelayServer, 3).ToList(),
            PublicEndpoint = Session.RemoteEndPoint,
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
            if (!ReferenceEquals(member, Session))
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
            if (!ReferenceEquals(member, Session))
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
            if (!ReferenceEquals(member, Session))
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
            .Select(m => locator.FindByKeys(m.SessionKey1, m.SessionKey2))
            .Where(s => s is not null)
            .Cast<IRelayPeer>()
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
            .Select(m => locator.FindByKeys(m.SessionKey1, m.SessionKey2))
            .Where(s => s is not null)
            .Cast<IRelayPeer>()
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
