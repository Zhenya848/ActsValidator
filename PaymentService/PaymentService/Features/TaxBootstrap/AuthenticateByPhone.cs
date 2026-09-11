using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PaymentMessaging.Contracts.Messaging;
using PaymentService.Abstractions;
using PaymentService.Extensions;
using PaymentService.Models.Shared;
using PaymentService.Models.Shared.ValueObjects.Id;
using PaymentService.Models.ValueObjects;
using PaymentService.Models.YandexKassa;
using PaymentService.Options;
using PaymentService.Outbox;
using PaymentService.Providers.MyTax;

namespace PaymentService.Features.TaxBootstrap;

public class AuthenticateByPhone
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/Payments/bootstrap/auth-by-phone-number", Handler);
        }
        
        private static async Task<IResult> Handler(
            [FromQuery] string challengeToken,
            [FromQuery] string code,
            IOptions<TaxAuthOptions> options,
            IMoyNalogBootstrap bootstrap,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            if (user.HasUserPermission("tax.bootstrap") == false)
                return Results.Forbid();
            
            await bootstrap.AuthenticateByPhoneAsync(
                options.Value.PhoneNumber, 
                challengeToken, 
                code, 
                cancellationToken);
 
            return Results.Ok(new { message = "Авторизация прошла успешно, refreshToken сохранён." });
        }
    }
}