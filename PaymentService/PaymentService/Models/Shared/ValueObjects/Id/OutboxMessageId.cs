namespace PaymentService.Models.Shared.ValueObjects.Id;

public record OutboxMessageId
{
    public Guid Value { get; }
    
    public OutboxMessageId(Guid value) => Value = value;
    
    public static OutboxMessageId AddNewId() => new (Guid.NewGuid());
    
    public static OutboxMessageId AddEmptyId() => new (Guid.Empty);
    
    public static OutboxMessageId Create(Guid id) => new (id);

    public static implicit operator Guid(OutboxMessageId id)
    {
        ArgumentNullException.ThrowIfNull(id);
        
        return id.Value;
    }
}