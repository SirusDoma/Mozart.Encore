using System.Text;
using Identity.Messages.Requests;
using Identity.Messages.Responses;
using Identity.Messages.Events;
using Encore.Server;
using Microsoft.Extensions.Logging;
using Mozart.Entities;
using Mozart.Services;
using Mozart.Sessions;

namespace Identity.Controllers;

[Authorize]
public class MessagingController(
    Session session,
    IChannelService channelService,
    ILogger<MessagingController> logger
) : CommandController<Session>(session)
{
    [CommandHandler]
    public async Task SendMainRoomUserMessage(SendMainRoomMessageRequest request, CancellationToken cancellationToken)
    {
        var actor = Session.GetAuthorizedToken<Actor>();
        logger.LogInformation((int)RequestCommand.SendMainRoomMessage,
            "[{Sender}] Send channel message: {Content}", actor.Nickname, Encoding.UTF8.GetString(request.Content));

        if (actor.IsAdministrator)
        {
            var message = new MainRoomAdminMessageResponse
            {
                Sender  = actor.Nickname,
                Content = request.Content
            };

            await channelService.Broadcast(message, cancellationToken);
        }
        else
        {
            var message = new MainRoomUserMessageResponse
            {
                Sender  = actor.Nickname,
                Content = request.Content
            };

            await channelService.Broadcast(message, cancellationToken);
        }
    }

    [CommandHandler]
    public async Task SendWaitingUserMessage(SendWaitingMessageRequest request, CancellationToken cancellationToken)
    {
        var actor = Session.GetAuthorizedToken<Actor>();
        logger.LogInformation((int)RequestCommand.SendWaitingMessage,
            "[{Sender}] Send room message: {Content}", actor.Nickname, Encoding.UTF8.GetString(request.Content));

        var room = Session.Room!;
        if (actor.IsAdministrator)
        {
            var message = new WaitingAdminMessageResponse
            {
                Sender  = actor.Nickname,
                Content = request.Content
            };

            await room.Broadcast(message, cancellationToken);
        }
        else
        {
            var message = new WaitingUserMessageResponse
            {
                Sender   = actor.Nickname,
                Content  = request.Content,
                MemberId = (byte)room.Slots.ToList().FindIndex(s =>
                    s is Room.MemberSlot m && m.Session == Session)
            };

            await room.Broadcast(message, cancellationToken);
        }
    }

    [CommandHandler]
    public async Task<SendWhisperResponse> SendWhisperMessage(SendWhisperMessageRequest request,
        CancellationToken cancellationToken)
    {
        var actor = Session.GetAuthorizedToken<Actor>();
        logger.LogInformation((int)RequestCommand.SendWhisper,
            "[{Sender}] Send whisper message to [{Recipient}]: {Content}", actor.Nickname, request.Recipient, request.Content);

        int recipientCount = await channelService.Broadcast(
            session => session.Actor.Nickname == request.Recipient,
            new WhisperEventData
            {
                Sender  = actor.Nickname,
                Content = request.Content
            },
        cancellationToken);

        return new SendWhisperResponse
        {
            Invalid   = recipientCount == 0,
            Recipient = request.Recipient,
            Content   = recipientCount == 0 ? string.Empty : request.Content
        };
    }

    [CommandHandler]
    public async Task Announce(AnnouncementRequest request, CancellationToken cancellationToken)
    {
        var actor = Session.GetAuthorizedToken<Actor>();
        logger.LogInformation((int)RequestCommand.Announce,
            "[{Sender}] Send announcement: {Content}", actor.Nickname, request.Content);

        foreach (var channel in channelService.GetChannels())
        {
            await channel.Broadcast(new AnnouncementResponse
            {
                Content = request.Content
            }, cancellationToken);
        }
    }
}
