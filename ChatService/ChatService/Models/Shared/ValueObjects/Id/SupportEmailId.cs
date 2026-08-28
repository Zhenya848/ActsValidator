namespace ChatService.Models.Shared.ValueObjects.Id;

public record SupportEmailId
{
    public Guid Value { get; }
    
    public SupportEmailId(Guid value) => Value = value;
    
    public static SupportEmailId AddNewId() => new (Guid.NewGuid());
    
    public static SupportEmailId AddEmptyId() => new (Guid.Empty);
    
    public static SupportEmailId Create(Guid id) => new (id);

    public static implicit operator Guid(SupportEmailId id)
    {
        ArgumentNullException.ThrowIfNull(id);
        
        return id.Value;
    }
}