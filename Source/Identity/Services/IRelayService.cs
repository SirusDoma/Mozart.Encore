using Memoryer.Messages.Requests;
using Memoryer.Relay.Messages.Requests;

namespace Memoryer.Services;

public interface IRelayService
{
    Task CreateSession(CreateRelaySessionRequest request, CancellationToken cancellationToken);

    Task DeleteSession(DeleteRelaySessionRequest request, CancellationToken cancellationToken);
}
