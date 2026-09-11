using System.Security.Claims;
using Elastic.CommonSchema;
using Microsoft.Extensions.Options;
using PaymentService.Abstractions;
using PaymentService.Extensions;
using PaymentService.Options;
using PaymentService.Providers.MyTax;

namespace PaymentService.Features.TaxBootstrap;

public class SendRequestCode
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/Payments/bootstrap/send-request-code", Handler);
        }
        
        private static async Task<IResult> Handler(
            IMoyNalogBootstrap bootstrap,
            ClaimsPrincipal user,
            IOptions<TaxAuthOptions> options,
            CancellationToken cancellationToken = default)
        {
            if (user.HasUserPermission("tax.bootstrap") == false)
                return Results.Forbid();
            
            var challengeToken = await bootstrap.RequestSmsCodeAsync(options.Value.PhoneNumber, cancellationToken);
            return Results.Ok(new { challengeToken });
        }
    }
}