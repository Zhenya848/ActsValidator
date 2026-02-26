namespace ActsValidator.Domain.Shared.ValueObjects.Ids;

public record AiRequestId
{
    public Guid Value { get; }
    
    public AiRequestId(Guid value) => Value = value;
    
    public static AiRequestId AddNewId() => new (Guid.NewGuid());
    
    public static AiRequestId AddEmptyId() => new (Guid.Empty);
    
    public static AiRequestId Create(Guid id) => new (id);

    public static implicit operator Guid(AiRequestId id)
    {
        ArgumentNullException.ThrowIfNull(id);
        
        return id.Value;
    }
}