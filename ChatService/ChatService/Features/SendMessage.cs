using System.Security.Claims;
using System.Text.Json;
using ChatService.Abstractions;
using ChatService.DbContexts;
using ChatService.Extensions;
using ChatService.Models.Chats;
using ChatService.Models.Event;
using ChatService.Models.Outbox;
using ChatService.Models.Shared;
using ChatService.Models.Shared.ValueObjects.Dtos;
using ChatService.Models.Shared.ValueObjects.Id;
using ChatService.Models.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace ChatService.Features;

public class SendMessage
{
    private record SendMessageRequest(string Message, Guid? ChatId = null);
    
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
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var userId = user.GetUserIdRequired();
        var isSupport = user.HasUserPermission("chat.all");

        var chatResult = await dbContext.Chats
            .Where(c => c.Id == request.ChatId)
            .FirstOrDefaultAsync(cancellationToken);

        if (chatResult is null)
        {
            if (isSupport == false)
            {
                var newChatResult = Chat.Create(userId);
                
                if (newChatResult.IsFailure)
                    return Results.BadRequest(newChatResult.Error);
                
                chatResult = newChatResult.Value;
            }
            else
                return Results.BadRequest();
        }

        if (isSupport == false && chatResult.UserId != userId)
            return Results.Forbid();
        
        var messageResult = Message.Create(
            chatResult.Id, 
            isSupport ? SenderType.Support : SenderType.Client,
            request.Message);
        
        if (messageResult.IsFailure)
            return Results.BadRequest(messageResult.Error);
        
        var message = messageResult.Value;
        chatResult.AddMessage(message);

        if (isSupport == false)
        {
            var sendEvent = new MessageWasSentEvent(
                message.Content, 
                chatResult.Id, 
                chatResult.UserId,
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
            chatResult.Id, 
            message.Type.ToString(), 
            message.Content, 
            message.IsRedacted,
            message.CreatedAt);

        return Results.Ok(result.Value);
    }
}