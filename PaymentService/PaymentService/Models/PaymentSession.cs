using System.Net.Mail;
using CSharpFunctionalExtensions;
using PaymentService.Models.Shared;
using PaymentService.Models.Shared.ValueObjects.Id;
using PaymentService.Models.ValueObjects;
using PaymentService.Models.Shared;

namespace PaymentService.Models;

public class PaymentSession : Shared.Entity<PaymentSessionId>
{
    public Guid UserId { get; init; }
    public string UserEmail { get; init; }
    
    public Product Product { get; init; }
    public string ProductId { get; init; }
    public PaymentSessionStatus Status { get; private set; } = PaymentSessionStatus.Created;
    public DateTime CreatedAt { get; init; }

    private PaymentSession(PaymentSessionId id) :  base(id)
    {
        
    }
    
    private PaymentSession(PaymentSessionId id, Guid userId, string userEmail, string productId, DateTime createdAt) : base(id)
    {
        UserId = userId;
        UserEmail = userEmail;
        ProductId = productId;
        CreatedAt = createdAt;
    }

    public static Result<PaymentSession, Error> Create(
        PaymentSessionId id, 
        Guid userId, 
        string userEmail,
        string productId,
        DateTime createdAt)
    {
        if (MailAddress.TryCreate(userEmail, out _) == false)
            return Errors.General.ValueIsInvalid(nameof(userEmail));
        
        if (string.IsNullOrWhiteSpace(productId))
            return Errors.General.ValueIsRequired(nameof(productId));
        
        if (createdAt < DateTime.UtcNow.AddMinutes(-5))
            return Errors.General.ValueIsInvalid(nameof(createdAt));
        
        return new PaymentSession(id, userId, userEmail, productId, createdAt);
    }
    
    public void Pending() => Status = PaymentSessionStatus.Pending;
    public void Failed() => Status = PaymentSessionStatus.Failed;
    public void Completed() => Status = PaymentSessionStatus.Completed;
}