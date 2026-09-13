using CrossTime.Messages.Requests;
using Encore.Data.Entities;
using Encore.Messaging;
using Mozart.Data.Entities;
using Mozart.Sessions;

namespace Encore.Controllers.Internal;

public partial class ChannelController
{
    private partial Actor CreateActor(User characterInfo, AuthSession authSession) => new(characterInfo)
    {
        Token    = authSession.Token,
        ClientId = string.Empty
    };

    private partial IMessage CreateChannelLoginRequest(ushort planet, ushort channel) => new ChannelLoginRequest
    {
        ServerId  = (short)planet,
        ChannelId = (short)channel
    };
}
