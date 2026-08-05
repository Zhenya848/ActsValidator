using ChatService.Abstractions;
using ChatService.Extensions;

namespace ChatService.Features;

public class SendMessage
{
    private record SendMessageRequest(string Message, Guid? ChatId = null);
    
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/Payments/create", Handler);
        }
    }

    private static async Task<IResult> Handler(
        SendMessageRequest request,
        IHttpContextAccessor httpContextAccessor)
    {
        var userId = httpContextAccessor.HttpContext?.User.GetUserIdRequired();
        
        
    }
}