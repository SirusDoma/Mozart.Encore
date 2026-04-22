using System.Net;
using Encore.Server;

namespace Encore.Sessions;

public interface IUdpSession : ISession
{
    UdpOptions Options { get; }

    IPEndPoint RemoteEndPoint { get; }
}
