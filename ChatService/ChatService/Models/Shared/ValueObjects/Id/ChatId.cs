namespace ChatService.Models.Shared.ValueObjects.Id;

public record ChatId
{
    public Guid Value { get; }
    
    public ChatId(Guid value) => Value = value;
    
    public static ChatId AddNewId() => new (Guid.NewGuid());
    
    public static ChatId AddEmptyId() => new (Guid.Empty);
    
    public static ChatId Create(Guid id) => new (id);

    public static implicit operator Guid(ChatId id)
    {
        ArgumentNullException.ThrowIfNull(id);
        
        return id.Value;
    }
}