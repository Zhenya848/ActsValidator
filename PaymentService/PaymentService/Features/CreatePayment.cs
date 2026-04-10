using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PaymentService.Abstractions;
using PaymentService.DbContexts;
using PaymentService.Extensions;
using PaymentService.Models;
using PaymentService.Models.Shared;
using PaymentService.Models.Shared.ValueObjects.Id;
using PaymentService.Options;
using Yandex.Checkout.V3;
using Error = PaymentService.Models.Shared.Error;


namespace PaymentService.Features;

public class CreatePayment
{
    private record CreatePaymentRequest(string ProductId);
    
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/Payments/create", Handler);
        }
    }

    private static async Task<IResult> Handler(
        CreatePaymentRequest request, 
        IHttpContextAccessor httpContextAccessor,
        IPublishEndpoint publishEndpoint,
        IOptions<YandexKassaOptions> options,
        ILogger<CreatePayment> logger,
        IUnitOfWork unitOfWork,
        AppDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        var userId = httpContextAccessor.HttpContext?.User.GetUserId();
        
        if (userId is null)
            return Results.Unauthorized();
        
        var userEmailVerified = httpContextAccessor.HttpContext?.User.GetUserEmailVerified();

        if (bool.TryParse(userEmailVerified, out bool userEmailVerifiedResult) && userEmailVerifiedResult == false)
            return Errors.User.NotVerified().ToIResultResponse();
        
        var product = await dbContext.Products
            .FirstOrDefaultAsync(i => i.Id == request.ProductId, cancellationToken);

        if (product is null)
            return Error.NotFound("product.not.found", $"product {request.ProductId} not found").ToIResultResponse();
        
        var paymentSessionResult = PaymentSession
            .Create(PaymentSessionId.AddNewId(), userId.Value, product.Id, DateTime.UtcNow);
        
        if (paymentSessionResult.IsFailure)
            return paymentSessionResult.Error.ToIResultResponse();
        
        var paymentSession = paymentSessionResult.Value;
        
        using var transaction = await unitOfWork.BeginTransaction(cancellationToken);

        var createResult = dbContext.PaymentSessions.Add(paymentSession);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        try
        {
            var client = new Yandex.Checkout.V3.Client(
                shopId: options.Value.ShopId.ToString(),
                secretKey: options.Value.SecretKey);

            AsyncClient asyncClient = client.MakeAsync();

            var newPayment = new NewPayment
            {
                Amount = new Amount { Value = product.Price, Currency = "RUB" },
                Confirmation = new Confirmation
                {
                    Type = ConfirmationType.Redirect,
                    ReturnUrl = "https://synapzeai.ru/tests"
                },
                Metadata = new Dictionary<string, string>()
                {
                    {"paymentSessionId", createResult.Entity.Id.Value.ToString()}
                },
                Capture = true
            };
            Payment payment = await asyncClient.CreatePaymentAsync(
                newPayment,
                Guid.NewGuid().ToString(),
                cancellationToken);

            string url = payment.Confirmation.ConfirmationUrl;
            
            transaction.Commit();

            return Results.Ok(Envelope.Ok(url));
        }
        catch (Exception ex)
        {
            logger.LogError("Ошибка при создании платежа: " + ex.Message);
            transaction.Rollback();

            return Error.Failure("payment.is.failure", "Произошла ошибка при создании платежа").ToIResultResponse();
        }
    }
}