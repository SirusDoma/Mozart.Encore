using Encore.Server;
using Encore.Sessions;

namespace Mozart;

[Authorize]
public class PlanetController(Session session) : CommandController(session)
{
    [CommandHandler(RequestCommand.GetChannelList)]
    public Task<ChannelListResponse> GetChannelList(CancellationToken cancellationToken)
    {
        return Task.FromResult(new ChannelListResponse
        {
            Channels =
            [
                new ()
                {
                    ServerId   = 0,
                    ChannelId  = 0,
                    Capacity   = 120,
                    Population = 0,
                    Active     = true
                },
                new ()
                {
                    ServerId   = 0,
                    ChannelId  = 1,
                    Capacity   = 120,
                    Population = 100,
                    Active     = true
                },
                new ()
                {
                    ServerId   = 0,
                    ChannelId  = 2,
                    Capacity   = 50,
                    Population = 40,
                    Active     = true
                },
                new ()
                {
                    ServerId   = 0,
                    ChannelId  = 2,
                    Capacity   = 150,
                    Population = 40,
                    Active     = false
                },
                new ()
                {
                    ServerId   = 0,
                    ChannelId  = 2,
                    Capacity   = 180,
                    Population = 150,
                    Active     = true
                }
            ]
        });
    }

    [CommandHandler]
    public Task<ChannelLoginResponse> ChannelLogin(ChannelLoginRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new ChannelLoginResponse { Full = false });
    }
}