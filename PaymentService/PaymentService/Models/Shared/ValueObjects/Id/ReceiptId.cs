namespace PaymentService.Models.Shared.ValueObjects.Id;

public record ReceiptId
{
    public Guid Value { get; }
    
    public ReceiptId(Guid value) => Value = value;
    
    public static ReceiptId AddNewId() => new (Guid.NewGuid());
    
    public static ReceiptId AddEmptyId() => new (Guid.Empty);
    
    public static ReceiptId Create(Guid id) => new (id);

    public static implicit operator Guid(ReceiptId id)
    {
        ArgumentNullException.ThrowIfNull(id);
        
        return id.Value;
    }
}