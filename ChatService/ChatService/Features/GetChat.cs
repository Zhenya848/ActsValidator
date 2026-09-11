using System.Security.Claims;
using ChatService.Abstractions;
using ChatService.DbContexts;
using ChatService.Extensions;
using ChatService.Models.Chats;
using ChatService.Models.Event;
using ChatService.Models.Shared;
using ChatService.Models.Shared.ValueObjects.Dtos;
using ChatService.Models.Shared.ValueObjects.Id;
using ChatService.Models.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace ChatService.Features;

public class GetChat
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("api/Chats/get", Handler);
        }
    }
    
    private static async Task<IResult> Handler(
        AppDbContext dbContext,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var userId = user.GetUserId();

        if (userId is null)
            return Results.Unauthorized();

        var chatResult = await dbContext.Chats
            .Where(c => c.User.Id == userId)
            .Include(m => m.Messages)
            .FirstOrDefaultAsync(cancellationToken);

        if (chatResult is null)
        {
            var userName = user.GetUserNameRequired();
            var userEmail = user.GetUserEmailRequired();

            chatResult = new Chat(
                UserData.Create(userId.Value, userName, userEmail).Value,
                ChatId.AddNewId());
            
            dbContext.Chats.Add(chatResult);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var messages = chatResult.Messages.OrderBy(c => c.CreatedAt).Select(m => MessageDto.Create(
            MessageId.Create(m.Id),
            ChatId.Create(m.ChatId),
            m.Type.ToString(), m.Content,
            m.IsRedacted,
            m.CreatedAt).Value)
            .ToArray();

        var result = new ChatDto(
            chatResult.Id, 
            UserData.Create(chatResult.User.Id, chatResult.User.Name, chatResult.User.Email).Value, 
            messages);

        return Results.Ok(Envelope.Ok(result));
    }
}