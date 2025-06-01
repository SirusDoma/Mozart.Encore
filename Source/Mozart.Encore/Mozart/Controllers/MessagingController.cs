using Encore.Messaging;
using Encore.Server;
using Encore.Sessions;

namespace Mozart;

[Authorize]
public class MessagingController(Session session, IMessageCodec codec) : CommandController(session)
{
    private static string _recipient = string.Empty;

    [CommandHandler]
    public Task<MainRoomUserMessageResponse> SendMainRoomUserMessage(SendMainRoomMessageRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new MainRoomUserMessageResponse
        {
            Sender  = Session.GetAuthorizedToken<CharacterInfo>().Nickname,
            Content = request.Content
        });
    }

    [CommandHandler]
    public Task<WaitingUserMessageResponse> SendWaitingUserMessage(SendWaitingMessageRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new WaitingUserMessageResponse()
        {
            Sender  = Session.GetAuthorizedToken<CharacterInfo>().Nickname,
            Content = request.Content
        });
    }

    [CommandHandler]
    public Task<SendWhisperResponse> SendWhisperMessage(SendWhisperMessageRequest request,
        CancellationToken cancellationToken)
    {
        _recipient = request.Recipient;
        return Task.FromResult(new SendWhisperResponse()
        {
            Invalid = false,
            Sender  = request.Recipient,
            Content = request.Content
        });
    }

    [CommandHandler]
    public async Task<ReceiveWhisperResponse> AutoReplyWhisper(SendWhisperMessageRequest request,
        CancellationToken cancellationToken)
    {
        int delay = Random.Shared.Next(700, 1000);
        await Task.Delay(delay, cancellationToken);

        if (Random.Shared.Next(0, 10_000) < 2_500)
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(Random.Shared.Next(delay + 250, delay + 500), cancellationToken);
                await Session.WriteFrameAsync(
                    codec.Encode(new AnnouncementResponse()
                    {
                        Content = $"{Session.GetAuthorizedToken<CharacterInfo>().Nickname} please stop " +
                                  $"harassing {_recipient}!"
                    }),
                    cancellationToken);
            }, cancellationToken);
        }

        return new ReceiveWhisperResponse()
        {
            Sender  = _recipient,
            Content = "I'm busy, go talk to someone else!"
        };
    }

    [CommandHandler]
    public Task<AnnouncementResponse> Announce(AnnouncementRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new AnnouncementResponse()
        {
            Content = request.Content
        });
    }
}