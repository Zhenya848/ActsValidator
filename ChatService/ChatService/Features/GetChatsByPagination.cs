using System.Security.Claims;
using ChatService.Abstractions;
using ChatService.DbContexts;
using ChatService.Extensions;
using ChatService.Models.Shared;
using ChatService.Models.Shared.ValueObjects.Dtos;
using ChatService.Models.Shared.ValueObjects.Id;
using ChatService.Models.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatService.Features;

public class GetChatsByPagination
{
    private record GetChatsByPaginationQuery(int Page, int PageSize, string? SearchByName);
    
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("api/Chats/get-by-pagination", Handler);
        }
    }
    
    private static async Task<IResult> Handler(
        AppDbContext dbContext,
        [AsParameters] GetChatsByPaginationQuery query,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var userId = user.GetUserId();

        if (userId is null)
            return Results.Unauthorized();
        
        var isSupport = user.HasUserPermission("chat.all");

        if (isSupport == false)
            return Results.Forbid();

        var chatsResult = await dbContext.Chats
            .Where(c => c.Messages.Any() 
                        && (query.SearchByName == null 
                            || c.User.Name.Contains(query.SearchByName) 
                            || c.User.Email.Contains(query.SearchByName)))
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Include(m => m.Messages)
            .ToListAsync(cancellationToken);

        var chats = chatsResult.Select(c => new ChatDto(
            c.Id, 
            UserData.Create(c.User.Id, c.User.Name, c.User.Email).Value, 
            c.Messages.OrderBy(ca => ca.CreatedAt).Select(m => MessageDto.Create(
                m.Id, 
                m.ChatId, 
                m.Type.ToString(), 
                m.Content, 
                m.IsRedacted, 
                m.CreatedAt).Value).ToArray()));
        
        var totalCount = await dbContext.Chats
            .Where(c => c.Messages.Any() 
                        && (query.SearchByName == null 
                            || c.User.Name.Contains(query.SearchByName) 
                            || c.User.Email.Contains(query.SearchByName)))
            .CountAsync(cancellationToken);

        return Results.Ok(new PagedList<ChatDto>()
        {
            Items = chats.ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        });
    }
}