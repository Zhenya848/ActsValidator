using AiService.Abstractions;
using AiService.Extensions;
using AiService.Providers;
using Microsoft.AspNetCore.Authorization;

namespace AiService.Features;

public class SendHandler
{
    private record SendRequest(string Prompt, string Base64File);
    
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/AI/send", Handler);
        }
        
        private static async Task<IResult> Handler(
            SendRequest request, 
            AiProvider aiProvider, 
            CancellationToken cancellationToken = default)
        {
            var aiResponse = await aiProvider
                .SendToAi(request.Prompt, request.Base64File, cancellationToken);

            if (aiResponse.IsFailure)
                return aiResponse.Error.ToIResultResponse();
            
            return Results.Ok(Envelope.Ok(aiResponse.Value));
        }
    }
}