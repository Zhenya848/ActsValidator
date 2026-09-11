using System.Security.Claims;
using System.Text.Json;
using ChatService.Abstractions;
using ChatService.DbContexts;
using ChatService.EmailSendingOutbox;
using ChatService.Extensions;
using ChatService.Hubs;
using ChatService.Models.Chats;
using ChatService.Models.Event;
using ChatService.Models.Shared;
using ChatService.Models.Shared.ValueObjects.Dtos;
using ChatService.Models.Shared.ValueObjects.Id;
using ChatService.Models.ValueObjects;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatService.Features;

public class SendMessage
{
    private record SendMessageRequest(
        string Message, 
        Guid? ChatId = null, 
        string? ConnectionId = null);
    
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/Chats/send", Handler);
        }
    }

    private static async Task<IResult> Handler(
        AppDbContext dbContext,
        SendMessageRequest request,
        IHubContext<ChatHub> hubContext,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var userId = user.GetUserId();

        if (userId is null)
            return Results.Unauthorized();
        
        var isSupport = user.HasUserPermission("chat.all");

        var chatResult = await dbContext.Chats
            .Where(c => c.Id == request.ChatId)
            .FirstOrDefaultAsync(cancellationToken);

        if (chatResult is null)
            return Errors.General.NotFound(request.ChatId).ToIResultResponse();

        if (isSupport == false && chatResult.User.Id != userId)
            return Error.Conflict("user.has.no.access", "User has no access to this chat").ToIResultResponse();
        
        var messageResult = Message.Create(
            chatResult.Id, 
            isSupport ? SenderType.Support : SenderType.Client,
            request.Message);
        
        if (messageResult.IsFailure)
            return messageResult.Error.ToIResultResponse();
        
        var message = messageResult.Value;
        chatResult.AddMessage(message);

        if (isSupport == false)
        {
            var sendEvent = new MessageWasSentEvent(
                message.Content, 
                chatResult.Id, 
                chatResult.User.Id,
                DateTime.UtcNow);
        
            var outboxMessage = new OutboxMessage(
                OutboxMessageId.AddNewId(),
                typeof(MessageWasSentEvent).AssemblyQualifiedName!,
                JsonSerializer.Serialize(sendEvent),
                DateTime.UtcNow);
            
            dbContext.OutboxMessages.Add(outboxMessage);
        }
        
        await dbContext.SaveChangesAsync(cancellationToken);

        var result = MessageDto.Create(
            message.Id,
            chatResult.Id, 
            message.Type.ToString(), 
            message.Content, 
            message.IsRedacted,
            message.CreatedAt);

        if (string.IsNullOrWhiteSpace(request.ConnectionId) == false)
        {
            await hubContext.Clients
                .GroupExcept($"chat:{chatResult.Id.Value}", [request.ConnectionId])
                .SendAsync(
                    "MessageReceived",
                    result.Value,
                    cancellationToken);
        }
        else
        {
            await hubContext.Clients
                .Group($"chat:{chatResult.Id.Value}")
                .SendAsync(
                    "MessageReceived",
                    result.Value,
                    cancellationToken);
        }

        return Results.Ok(Envelope.Ok(result.Value));
    }
}