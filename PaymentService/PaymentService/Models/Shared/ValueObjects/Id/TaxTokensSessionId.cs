namespace PaymentService.Models.Shared.ValueObjects.Id;

public record TaxTokensSessionId
{
    public Guid Value { get; }
    
    public TaxTokensSessionId(Guid value) => Value = value;
    
    public static TaxTokensSessionId AddNewId() => new (Guid.NewGuid());
    
    public static TaxTokensSessionId AddEmptyId() => new (Guid.Empty);
    
    public static TaxTokensSessionId Create(Guid id) => new (id);

    public static implicit operator Guid(TaxTokensSessionId id)
    {
        ArgumentNullException.ThrowIfNull(id);
        
        return id.Value;
    }
}