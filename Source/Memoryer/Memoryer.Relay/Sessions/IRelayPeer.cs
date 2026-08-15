using System.Net;
using Encore.Messaging;
using Encore.Sessions;

namespace Memoryer.Relay.Sessions;

public interface IRelayPeer : ISession
{
    IPEndPoint LocalEndPoint { get; }

    IPEndPoint RemoteEndPoint { get; }

    bool Authorized { get; }

    void Authorize<T>(T token);

    T GetAuthorizedToken<T>();

    Task WriteMessage<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : class, IMessage;
}
