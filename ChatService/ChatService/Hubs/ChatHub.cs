using ChatService.DbContexts;
using ChatService.Extensions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatService.Hubs;

public class ChatHub : Hub
{
    private readonly AppDbContext _dbContext;

    public ChatHub(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task JoinChat(Guid chatId)
    {
        var userId = Context.User?.GetUserId();

        if (userId is null)
            throw new HubException("NotAuthorized");

        var canAccess = Context.User?.HasUserPermission("chat.all");

        switch (canAccess)
        {
            case null:
                throw new HubException("PermissionDenied");
            case false:
            {
                var isUserHasChat = await _dbContext.Chats
                    .AnyAsync(c => c.Id == chatId && c.User.Id == userId);

                if (!isUserHasChat)
                    throw new HubException("Forbidden");
                
                break;
            }
        }

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            $"chat:{chatId}");
    }
}