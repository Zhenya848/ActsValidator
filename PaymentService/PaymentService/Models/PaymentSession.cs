using Core;
using CSharpFunctionalExtensions;
using PaymentService.Models.Shared;
using PaymentService.Models.Shared.ValueObjects.Id;

namespace PaymentService.Models;

public class PaymentSession : Core.Entity<PaymentSessionId>
{
    public Guid UserId { get; init; }
    
    public Product Product { get; init; }
    public string ProductId { get; init; }

    private PaymentSession(PaymentSessionId id) :  base(id)
    {
        
    }
    
    private PaymentSession(PaymentSessionId id, Guid userId, string productId) : base(id)
    {
        UserId = userId;
        ProductId = productId;
    }

    public static Result<PaymentSession, Error> Create(
        PaymentSessionId id, 
        Guid userId, 
        string productId)
    {
        if (string.IsNullOrWhiteSpace(productId))
            return Errors.General.ValueIsRequired(nameof(productId));
        
        return new PaymentSession(id, userId, productId);
    }
}