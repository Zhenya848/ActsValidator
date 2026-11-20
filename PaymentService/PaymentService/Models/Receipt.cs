using System.Net.Mail;
using CSharpFunctionalExtensions;
using PaymentService.Models.Shared;
using PaymentService.Models.Shared.ValueObjects.Id;
using PaymentService.Models.ValueObjects;

namespace PaymentService.Models;

public class Receipt : Shared.Entity<ReceiptId>
{
    public decimal Amount { get; init; }
    public string Currency { get; init; } = null!;
    public string CustomerEmail { get; init; } = null!;
    
    public DateTime OccurredOn { get; }
    public DateTime? ProcessedOn { get; set; }
    
    public string? Error { get; set; }
    public Uri? PrintUrl { get; set; }
    
    private Receipt(ReceiptId id) : base(id)
    {
        
    }

    private Receipt(
        ReceiptId id, 
        decimal amount, 
        string currency, 
        string customerEmail,
        DateTime occurredOn) : base(id)
    {
        Amount = amount;
        Currency = currency;
        CustomerEmail = customerEmail;
        OccurredOn = occurredOn;
    } 

    public static Result<Receipt, Error> Create(
        decimal amount, 
        string currency, 
        string customerEmail,
        DateTime occurredOn)
    {
        if (amount <= 0)
            return Errors.General.ValueIsInvalid("amount");
        
        if (string.IsNullOrWhiteSpace(currency))
            return Errors.General.ValueIsRequired("currency");

        if (MailAddress.TryCreate(customerEmail, out _) == false)
            return Errors.General.ValueIsInvalid("customer email");
        
        if (occurredOn > DateTime.UtcNow)
            return Errors.General.ValueIsInvalid("occurred on");
        
        return new Receipt(ReceiptId.AddNewId(), amount, currency, customerEmail, occurredOn);
    }
}