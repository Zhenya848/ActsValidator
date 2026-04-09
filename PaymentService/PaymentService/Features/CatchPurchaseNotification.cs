using System.Text.Json;
using Application.Abstractions;
using Core;
using Core.Events;
using Framework.Extensions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using PaymentMessaging.Contracts.Messaging;
using PaymentService.Abstractions;
using PaymentService.DbContexts;
using PaymentService.Extensions;
using PaymentService.Models.Shared.ValueObjects.Id;
using PaymentService.Models.YandexKassa;
using PaymentService.Outbox;

namespace PaymentService.Features;

public class CatchPurchaseNotification
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/Payments/notification", Handler);
        }
        
        private static async Task<IResult> Handler(
            object request,
            AppDbContext dbContext,
            IUnitOfWork unitOfWork,
            IPublishEndpoint publishEndpoint,
            CancellationToken cancellationToken = default)
        {
            var requestResult = request.ConvertObjectToType<CatchNotificationRequest>();

            if (requestResult.IsFailure)
                return requestResult.Error.ToIResultResponse();
            
            var paymentDataRequest = requestResult.Value.Object;
            
            if (paymentDataRequest.Metadata.TryGetValue("paymentSessionId", out var paymentSessionIdStr) == false)
                return Errors.General.ValueIsRequired("paymentSessionId").ToIResultResponse();

            if (Guid.TryParse(paymentSessionIdStr, out var paymentSessionId) == false)
                return Errors.General.ValueIsInvalid("paymentSessionId").ToIResultResponse();

            var paymentSession = await dbContext.PaymentSessions
                .Include(p => p.Product)
                .FirstOrDefaultAsync(i => i.Id == paymentSessionId, cancellationToken);
            
            if (paymentSession is null)
                return Errors.General.NotFound(paymentSessionId).ToIResultResponse();
            
            var amount = int.Parse(paymentDataRequest.Amount.Value.Split('.')[0]);
            
            if (amount != paymentSession.Product.Price)
                return Error.Conflict("amount.not.match", "Сумма заказа и цена продукта не сходятся").ToIResultResponse();

            var userBoughtEvent = new ProductWasBoughtEvent(paymentSession.UserId, paymentSession.Product.Id);

            var outboxMessage = new OutboxMessage(
                OutboxMessageId.AddNewId(),
                userBoughtEvent.GetType().FullName!,
                JsonSerializer.Serialize(userBoughtEvent),
                DateTime.UtcNow);
            
            dbContext.OutboxMessages.Add(outboxMessage);
            paymentSession.Pending();
            
            await unitOfWork.SaveChanges(cancellationToken);
            
            return Results.Ok();
        }
    }
}