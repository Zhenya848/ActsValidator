namespace PaymentService.Models.Shared.ValueObjects.Id;

public record PaymentSessionId
{
    public Guid Value { get; }
    
    public PaymentSessionId(Guid value) => Value = value;
    
    public static PaymentSessionId AddNewId() => new (Guid.NewGuid());
    
    public static PaymentSessionId AddEmptyId() => new (Guid.Empty);
    
    public static PaymentSessionId Create(Guid id) => new (id);

    public static implicit operator Guid(PaymentSessionId id)
    {
        ArgumentNullException.ThrowIfNull(id);
        
        return id.Value;
    }
}