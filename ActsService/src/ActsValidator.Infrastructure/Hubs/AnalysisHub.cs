using Microsoft.AspNetCore.SignalR;

namespace ActsValidator.Infrastructure.Hubs;

public class AnalysisHub: Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        var isAuth = Context.User?.Identity?.IsAuthenticated;

        Console.WriteLine($"Hub connected: User={userId}, IsAuth={isAuth}"); 
        await base.OnConnectedAsync();
    }
}