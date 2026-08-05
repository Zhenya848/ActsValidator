namespace ChatService.Models.Shared.ValueObjects.Id;

public record MessageId
{
    public Guid Value { get; }
    
    public MessageId(Guid value) => Value = value;
    
    public static MessageId AddNewId() => new (Guid.NewGuid());
    
    public static MessageId AddEmptyId() => new (Guid.Empty);
    
    public static MessageId Create(Guid id) => new (id);

    public static implicit operator Guid(MessageId id)
    {
        ArgumentNullException.ThrowIfNull(id);
        
        return id.Value;
    }
}