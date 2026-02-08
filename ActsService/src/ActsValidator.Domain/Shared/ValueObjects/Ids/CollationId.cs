namespace ActsValidator.Domain.Shared.ValueObjects.Ids;

public record CollationId
{
    public Guid Value { get; }
    
    public CollationId(Guid value) => Value = value;
    
    public static CollationId AddNewId() => new (Guid.NewGuid());
    
    public static CollationId AddEmptyId() => new (Guid.Empty);
    
    public static CollationId Create(Guid id) => new (id);

    public static implicit operator Guid(CollationId id)
    {
        ArgumentNullException.ThrowIfNull(id);
        
        return id.Value;
    }
}