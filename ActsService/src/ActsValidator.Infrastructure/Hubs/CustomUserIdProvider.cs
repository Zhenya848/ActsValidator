using System.Security.Claims;
using ActsValidator.Presentation.Extensions;
using Microsoft.AspNetCore.SignalR;

namespace ActsValidator.Infrastructure.Hubs;

public class CustomUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        return connection.User.GetUserId()?.ToString()
               ?? connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}